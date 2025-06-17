using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Threading.Channels;
using System.Numerics;

namespace way.Services;

public class ImuSensorService : IDisposable
{
    private ImuData _lastAccelData;

    private Channel<ImuData> _dataChannel;
    private readonly CancellationTokenSource _cts = new();
    private Task _fileWriterTask;
    private IAccelerometer _accelerometer = Accelerometer.Default;
    private IOrientationSensor _orientation = OrientationSensor.Default;

    public bool IsRecording { get; private set; }
    public string CurrentFileName { get; private set; }

    public event EventHandler<ImuData> DataUpdated;

    public void StartRecording(string filePath)
    {
        if (IsRecording) return;

        _dataChannel = Channel.CreateBounded<ImuData>(
            new BoundedChannelOptions(4000)
            {
                SingleWriter = true,
                SingleReader = true,
                FullMode = BoundedChannelFullMode.DropOldest
            });

        CurrentFileName = filePath;
        IsRecording = true;

        _accelerometer.ReadingChanged += OnAccelerometerReadingChanged;
        _orientation.ReadingChanged += OnOrientationReadingChanged;

        _accelerometer.Start(SensorSpeed.Fastest);
        _orientation.Start(SensorSpeed.Fastest);

        _fileWriterTask = Task.Run(() => WriteToFileAsync(filePath, _cts.Token));
    }

    public async Task StopRecordingAsync()
    {
        if (!IsRecording) return;

        try
        {
            _accelerometer.ReadingChanged -= OnAccelerometerReadingChanged;
            _orientation.ReadingChanged -= OnOrientationReadingChanged;

            _accelerometer.Stop();
            _orientation.Stop();

            if (!_dataChannel.Reader.Completion.IsCompleted)
            {
                _dataChannel.Writer.TryComplete();
            }

            if (_fileWriterTask != null && !_fileWriterTask.IsCompleted)
            {
                await _fileWriterTask;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping recording: {ex}");
            throw;
        }
        finally
        {
            IsRecording = false;
        }
    }

    private void OnAccelerometerReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        if (!IsRecording) return;

        var reading = e.Reading;
        _lastAccelData = new ImuData
        {
            AccelX = reading.Acceleration.X,
            AccelY = reading.Acceleration.Y,
            AccelZ = reading.Acceleration.Z
        };
    }

    private void OnOrientationReadingChanged(object sender, OrientationSensorChangedEventArgs e)
    {
        if (!IsRecording) return;

        var reading = e.Reading;
        var orientation = reading.Orientation;
        var rotationMatrix = GetRotationMatrixFromQuaternion(orientation);

        var absoluteAcceleration = CalculateAbsoluteAcceleration(_lastAccelData, rotationMatrix);

        var imuData = new ImuData
        {
            AccelX = absoluteAcceleration.X,
            AccelY = absoluteAcceleration.Y,
            AccelZ = absoluteAcceleration.Z
        };

        DataUpdated?.Invoke(this, imuData);
        _dataChannel.Writer.TryWrite(imuData);
    }

    private Vector3 CalculateAbsoluteAcceleration(ImuData accelData, Matrix4x4 rotationMatrix)
    {
        // Преобразуем ускорение из системы координат устройства в мировую систему координат
        var deviceAcceleration = new Vector3(accelData.AccelX, accelData.AccelY, accelData.AccelZ);
        var worldAcceleration = Vector3.Transform(deviceAcceleration, rotationMatrix);

        worldAcceleration.Z -= 1.0f;

        return worldAcceleration;
    }

    private Matrix4x4 GetRotationMatrixFromQuaternion(Quaternion quaternion)
    {
        // Преобразуем кватернион в матрицу поворота
        return Matrix4x4.CreateFromQuaternion(quaternion);
    }

    private async Task WriteToFileAsync(string filePath, CancellationToken ct)
    {
        const int recordSize = 12;

        await using var file = File.Create(filePath);
        var pipeWriter = PipeWriter.Create(file);

        try
        {
            // Записываем заголовок файла (версия, частота дискретизации)
            var header = pipeWriter.GetSpan(8);
            BinaryPrimitives.WriteInt32LittleEndian(header, 1); // Версия формата
            BinaryPrimitives.WriteInt32LittleEndian(header[4..], 100); // Частота (Гц)
            pipeWriter.Advance(8);
            await pipeWriter.FlushAsync(ct);

            await foreach (var data in _dataChannel.Reader.ReadAllAsync(ct))
            {
                if (ct.IsCancellationRequested)
                    break;
                var buffer = pipeWriter.GetSpan(recordSize);
                BinaryPrimitives.WriteSingleLittleEndian(buffer, data.AccelX);
                BinaryPrimitives.WriteSingleLittleEndian(buffer[4..], data.AccelY);
                BinaryPrimitives.WriteSingleLittleEndian(buffer[8..], data.AccelZ);
                pipeWriter.Advance(recordSize);

                if (pipeWriter.UnflushedBytes > 8192)
                    await pipeWriter.FlushAsync(ct);
            }

            await pipeWriter.FlushAsync(ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in WriteToFileAsync: {ex}");
            throw;
        }
        finally
        {
            await pipeWriter.CompleteAsync();
        }
    }

    public static async IAsyncEnumerable<ImuData> StreamDataAsync(string filePath)
    {
        await using var file = File.OpenRead(filePath);
        var pipeReader = PipeReader.Create(file);

        try
        {
            // Пропускаем заголовок (8 байт)
            var headerResult = await pipeReader.ReadAsync();
            pipeReader.AdvanceTo(headerResult.Buffer.GetPosition(8));

            while (true)
            {
                var readResult = await pipeReader.ReadAsync();
                var buffer = readResult.Buffer;

                while (TryReadRecord(ref buffer, out var record))
                {
                    yield return record;
                }

                pipeReader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted)
                    break;
            }
        }
        finally
        {
            await pipeReader.CompleteAsync();
        }
    }

    private static bool TryReadRecord(ref ReadOnlySequence<byte> buffer, out ImuData record)
    {
        const int recordSize = 12;

        if (buffer.Length < recordSize)
        {
            record = default;
            return false;
        }

        var slice = buffer.Slice(0, recordSize);
        var data = new ImuData();

        if (slice.IsSingleSegment)
        {
            ReadRecord(slice.FirstSpan, ref data);
        }
        else
        {
            Span<byte> tempSpan = stackalloc byte[recordSize];
            slice.CopyTo(tempSpan);
            ReadRecord(tempSpan, ref data);
        }

        buffer = buffer.Slice(recordSize);
        record = data;
        return true;
    }

    private static void ReadRecord(ReadOnlySpan<byte> span, ref ImuData data)
    {
        data.AccelX = BinaryPrimitives.ReadSingleLittleEndian(span);
        data.AccelY = BinaryPrimitives.ReadSingleLittleEndian(span[4..]);
        data.AccelZ = BinaryPrimitives.ReadSingleLittleEndian(span[8..]);
    }

    public static async Task<long> GetRecordCountAsync(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        const int headerSize = 8;
        const int recordSize = 12;

        if (!fileInfo.Exists)
            return 0;

        return (fileInfo.Length - headerSize) / recordSize;
    }

    public void Dispose()
    {
        try
        {
            _cts.Cancel();

            if (IsRecording)
            {
                StopRecordingAsync().Wait();
            }

            _cts.Dispose();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }
}

public struct ImuData
{
    public float AccelX, AccelY, AccelZ; // Абсолютное линейное ускорение
}
