import os

from replit import db
from telebot import types, util
from telebot.async_telebot import AsyncTeleBot
from telebot.asyncio_handler_backends import State, StatesGroup

from Assets.filters import bind_filters, run_type, time_set
from Assets.keyboards import (
    DATE,
    EMTPY_FIELD,
    generate_excercise_name,
    generate_km,
    generate_lines,
    generate_metrs,
    generate_reps,
    generate_running_type,
    generate_sets,
    generate_time_set,
)

CANAL_ID = os.environ["canal_id"]


class Inputs(StatesGroup):
    exercise_name = State()
    sets = State()
    reps = State()
    lines = State()
    metrs = State()
    metrs_km = State()
    km = State()


async def command_start(message: types.Message, bot: AsyncTeleBot):
    await bot.send_message(
        chat_id=message.chat.id,
        text="🔥 THIS IS THE WAY 🔥\n📋 Используй меню команд",
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )


async def command_exercise(message: types.Message, bot: AsyncTeleBot):
    new_message = await bot.send_message(
        chat_id=message.chat.id,
        text=DATE().NOW(),
        reply_markup=generate_excercise_name(),
    )
    await bot.set_state(
        user_id=message.from_user.id,
        state=Inputs.exercise_name,
        chat_id=message.chat.id,
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )
    async with bot.retrieve_data(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    ) as data:
        data["id_train"] = new_message.id
        data["text_train"] = new_message.text


async def command_running(message: types.Message, bot: AsyncTeleBot):
    await bot.send_message(
        chat_id=message.chat.id,
        text=DATE().NOW(),
        reply_markup=generate_running_type(),
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )


async def command_delete(message: types.Message, bot: AsyncTeleBot):
    if str(message.chat.id) in db.keys():
        last_message_id = db[f"{message.chat.id}"][1]
        last_message_id_channel = db[message.from_user.username][1]
        await bot.delete_message(
            chat_id=message.chat.id,
            message_id=message.id,
        )
        await bot.delete_message(
            chat_id=message.chat.id,
            message_id=last_message_id,
        )
        del db[f"{message.chat.id}"]
        await bot.delete_message(
            chat_id=CANAL_ID,
            message_id=last_message_id_channel,
        )
        del db[message.from_user.username]
    else:
        await bot.delete_message(
            chat_id=message.chat.id,
            message_id=message.id,
        )


async def input_exercise_name(message: types.Message, bot: AsyncTeleBot):
    async with bot.retrieve_data(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    ) as data:
        old_text = data["text_train"]
        new_text = f"{old_text}\n\n🦾 {message.text.capitalize()}"
        data["text_train"] = new_text
        train_id = data["id_train"]
    await bot.edit_message_text(
        text=new_text,
        chat_id=message.chat.id,
        message_id=train_id,
        reply_markup=generate_sets(),
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )
    await bot.set_state(
        user_id=message.from_user.id,
        state=Inputs.sets,
        chat_id=message.chat.id,
    )


async def input_sets(message: types.Message, bot: AsyncTeleBot):
    async with bot.retrieve_data(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    ) as data:
        old_text = data["text_train"]
        new_text = f"{old_text}\n🟦Подходы: {message.text}"
        data["text_train"] = new_text
        data["sets"] = int(message.text)
        data["reps"] = []
        train_id = data["id_train"]
    await bot.edit_message_text(
        text=new_text,
        chat_id=message.chat.id,
        message_id=train_id,
        reply_markup=generate_reps(),
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )
    await bot.set_state(
        user_id=message.from_user.id,
        state=Inputs.reps,
        chat_id=message.chat.id,
    )


async def input_reps(message: types.Message, bot: AsyncTeleBot):
    delete_state_flag = False
    async with bot.retrieve_data(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    ) as data:
        sets = data["sets"]
        reps_lis = data["reps"]
        reps_lis.append(message.text)
        data["reps"] = reps_lis
        old_text = data["text_train"]
        new_text = f"{old_text}\n🔵 Повторения: {'-'.join(reps_lis)}"
        data["text_train"] = old_text
        train_id = data["id_train"]
    if len(reps_lis) == sets and sets == 1:
        await bot.edit_message_text(
            new_text,
            chat_id=message.chat.id,
            message_id=train_id,
        )
        await bot.delete_message(
            chat_id=message.chat.id,
            message_id=message.id,
        )
        if message.from_user.username not in db.keys():
            text_for_channel = f"@{message.from_user.username}\n{new_text}"
            message_for_channel = await bot.send_message(
                chat_id="@destiny_way",
                text=text_for_channel,
            )
            db[message.from_user.username] = (
                text_for_channel,
                message_for_channel.id,
            )
        else:
            last_message_text = db[message.from_user.username][0]
            last_message_id = db[message.from_user.username][1]
            if last_message_text.split("\n")[1] == new_text.split("\n")[0]:
                newpartmes = "\n".join(new_text.split("\n")[1:])
                new_mes = await bot.edit_message_text(
                    text=f"{last_message_text}\n{newpartmes}",
                    chat_id="@destiny_way",
                    message_id=last_message_id,
                )
                db[message.from_user.username] = (new_mes.text, new_mes.id)
            else:
                text_for_channel = f"@{message.from_user.username}\n{new_text}"
                message_for_channel = await bot.send_message(
                    chat_id="@destiny_way",
                    text=text_for_channel,
                )
                db[message.from_user.username] = (
                    text_for_channel,
                    message_for_channel.id,
                )

        if str(message.chat.id) not in db.keys():
            db[f"{message.chat.id}"] = (new_text, train_id)
        else:
            last_message_text = db[f"{message.chat.id}"][0]
            last_message_id = db[f"{message.chat.id}"][1]
            if last_message_text.split("\n")[0] == new_text.split("\n")[0]:
                newpartmes = "\n".join(new_text.split("\n")[1:])
                new_mes = await bot.edit_message_text(
                    text=f"{last_message_text}\n{newpartmes}",
                    chat_id=message.chat.id,
                    message_id=last_message_id,
                )
                await bot.delete_message(
                    chat_id=message.chat.id,
                    message_id=train_id,
                )
                db[f"{message.chat.id}"] = (new_mes.text, new_mes.id)
            else:
                db[f"{message.chat.id}"] = (new_text, train_id)
        delete_state_flag = True
    elif len(reps_lis) == sets and sets != 1:
        await bot.edit_message_text(
            new_text,
            chat_id=message.chat.id,
            message_id=train_id,
            reply_markup=generate_time_set(),
        )
        await bot.delete_message(
            chat_id=message.chat.id,
            message_id=message.id,
        )
        delete_state_flag = True
    elif len(reps_lis) != sets:
        await bot.edit_message_text(
            text=new_text,
            chat_id=message.chat.id,
            message_id=train_id,
            reply_markup=generate_reps(),
        )
        await bot.delete_message(
            chat_id=message.chat.id,
            message_id=message.id,
        )
    if delete_state_flag:
        await bot.delete_state(
            user_id=message.from_user.id,
            chat_id=message.chat.id,
        )


async def input_lines(message: types.Message, bot: AsyncTeleBot):
    async with bot.retrieve_data(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    ) as data:
        old_text = data["text_train"]
        new_text = f"{old_text}\n📏Расстояние: {message.text} X"
        data["text_train"] = new_text
        train_id = data["id_train"]
    await bot.edit_message_text(
        text=new_text,
        chat_id=message.chat.id,
        message_id=train_id,
        reply_markup=generate_metrs(),
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )
    await bot.set_state(
        user_id=message.from_user.id,
        state=Inputs.metrs,
        chat_id=message.chat.id,
    )


async def input_metrs(message: types.Message, bot: AsyncTeleBot):
    async with bot.retrieve_data(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    ) as data:
        old_text = data["text_train"]
        new_text = f"{old_text} {message.text} м"
        train_id = data["id_train"]
    await bot.edit_message_text(
        text=new_text,
        chat_id=message.chat.id,
        message_id=train_id,
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )
    if message.from_user.username not in db.keys():
        text_for_channel = f"@{message.from_user.username}\n{new_text}"
        message_for_channel = await bot.send_message(
            chat_id="@destiny_way",
            text=text_for_channel,
        )
        db[message.from_user.username] = (
            text_for_channel,
            message_for_channel.id,
        )
    else:
        last_message_text = db[message.from_user.username][0]
        last_message_id = db[message.from_user.username][1]
        if last_message_text.split("\n")[1] == new_text.split("\n")[0]:
            newpartmes = "\n".join(new_text.split("\n")[1:])
            new_mes = await bot.edit_message_text(
                text=f"{last_message_text}\n{newpartmes}",
                chat_id="@destiny_way",
                message_id=last_message_id,
            )
            db[message.from_user.username] = (new_mes.text, new_mes.id)
        else:
            text_for_channel = f"@{message.from_user.username}\n{new_text}"
            message_for_channel = await bot.send_message(
                chat_id="@destiny_way",
                text=text_for_channel,
            )
            db[message.from_user.username] = (
                text_for_channel,
                message_for_channel.id,
            )
    if str(message.chat.id) not in db.keys():
        db[f"{message.chat.id}"] = (new_text, train_id)
    else:
        last_message_text = db[f"{message.chat.id}"][0]
        last_message_id = db[f"{message.chat.id}"][1]
        if last_message_text.split("\n")[0] == new_text.split("\n")[0]:
            newpartmes = "\n".join(new_text.split("\n")[1:])
            new_mes = await bot.edit_message_text(
                text=f"{last_message_text}\n{newpartmes}",
                chat_id=message.chat.id,
                message_id=last_message_id,
            )
            await bot.delete_message(
                chat_id=message.chat.id,
                message_id=train_id,
            )
            db[f"{message.chat.id}"] = (new_mes.text, new_mes.id)
        else:
            db[f"{message.chat.id}"] = (new_text, train_id)
    await bot.delete_state(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    )


async def input_km(message: types.Message, bot: AsyncTeleBot):
    async with bot.retrieve_data(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    ) as data:
        old_text = data["text_train"]
        new_text = f"{old_text}\n📏Расстояние: {message.text} км"
        data["text_train"] = old_text
        data["km"] = int(message.text)
        train_id = data["id_train"]
    await bot.edit_message_text(
        text=new_text,
        chat_id=message.chat.id,
        message_id=train_id,
        reply_markup=generate_metrs(),
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )
    await bot.set_state(
        user_id=message.from_user.id,
        state=Inputs.metrs_km,
        chat_id=message.chat.id,
    )


async def input_metrs_km(message: types.Message, bot: AsyncTeleBot):
    async with bot.retrieve_data(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    ) as data:
        old_text = data["text_train"]
        km = data["km"]
        new_text = f"{old_text}\n📏Расстояние: {km*1000+int(message.text)} м"
        train_id = data["id_train"]
    await bot.edit_message_text(
        text=new_text,
        chat_id=message.chat.id,
        message_id=train_id,
    )
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )
    if message.from_user.username not in db.keys():
        text_for_channel = f"@{message.from_user.username}\n{new_text}"
        message_for_channel = await bot.send_message(
            chat_id="@destiny_way",
            text=text_for_channel,
        )
        db[message.from_user.username] = (
            text_for_channel,
            message_for_channel.id,
        )
    else:
        last_message_text = db[message.from_user.username][0]
        last_message_id = db[message.from_user.username][1]
        if last_message_text.split("\n")[1] == new_text.split("\n")[0]:
            newpartmes = "\n".join(new_text.split("\n")[1:])
            new_mes = await bot.edit_message_text(
                text=f"{last_message_text}\n{newpartmes}",
                chat_id="@destiny_way",
                message_id=last_message_id,
            )
            db[message.from_user.username] = (new_mes.text, new_mes.id)
        else:
            text_for_channel = f"@{message.from_user.username}\n{new_text}"
            message_for_channel = await bot.send_message(
                chat_id="@destiny_way",
                text=text_for_channel,
            )
            db[message.from_user.username] = (
                text_for_channel,
                message_for_channel.id,
            )
    if str(message.chat.id) not in db.keys():
        db[f"{message.chat.id}"] = (new_text, train_id)
    else:
        last_message_text = db[f"{message.chat.id}"][0]
        last_message_id = db[f"{message.chat.id}"][1]
        if last_message_text.split("\n")[0] == new_text.split("\n")[0]:
            newpartmes = "\n".join(new_text.split("\n")[1:])
            new_mes = await bot.edit_message_text(
                text=f"{last_message_text}\n{newpartmes}",
                chat_id=message.chat.id,
                message_id=last_message_id,
            )
            await bot.delete_message(
                chat_id=message.chat.id,
                message_id=train_id,
            )
            db[f"{message.chat.id}"] = (new_mes.text, new_mes.id)
        else:
            db[f"{message.chat.id}"] = (new_text, train_id)
    await bot.delete_state(
        user_id=message.from_user.id,
        chat_id=message.chat.id,
    )


async def type_running_callback_handler(call: types.CallbackQuery, bot: AsyncTeleBot):
    callback_data: dict = run_type.parse(callback_data=call.data)
    type_run = callback_data["type"]
    old_text = call.message.text
    new_text = f"{old_text}\n\n🦿 {type_run}"
    if type_run == "Интервалы":
        await bot.edit_message_text(
            text=new_text,
            chat_id=call.message.chat.id,
            message_id=call.message.id,
            reply_markup=generate_lines(),
        )
        await bot.set_state(
            user_id=call.from_user.id,
            state=Inputs.lines,
            chat_id=call.message.chat.id,
        )
        async with bot.retrieve_data(
            user_id=call.from_user.id,
            chat_id=call.message.chat.id,
        ) as data:
            data["id_train"] = call.message.id
            data["text_train"] = new_text
    else:
        await bot.edit_message_text(
            text=new_text,
            chat_id=call.message.chat.id,
            message_id=call.message.id,
            reply_markup=generate_km(),
        )
        await bot.set_state(
            user_id=call.from_user.id,
            state=Inputs.km,
            chat_id=call.message.chat.id,
        )
        async with bot.retrieve_data(
            user_id=call.from_user.id,
            chat_id=call.message.chat.id,
        ) as data:
            data["id_train"] = call.message.id
            data["text_train"] = new_text


async def time_set_callback_handler(call: types.CallbackQuery, bot: AsyncTeleBot):
    callback_data: dict = time_set.parse(callback_data=call.data)
    time = callback_data["time"]
    old_text = call.message.text
    new_text = f"{old_text}\n⏱ Отдых: {time}"
    await bot.edit_message_text(
        text=new_text,
        chat_id=call.message.chat.id,
        message_id=call.message.id,
    )
    if call.from_user.username not in db.keys():
        text_for_channel = f"@{call.from_user.username}\n{new_text}"
        message_for_channel = await bot.send_message(
            chat_id="@destiny_way",
            text=text_for_channel,
        )
        db[call.from_user.username] = (
            text_for_channel,
            message_for_channel.id,
        )
    else:
        last_message_text = db[call.from_user.username][0]
        last_message_id = db[call.from_user.username][1]
        if last_message_text.split("\n")[1] == new_text.split("\n")[0]:
            newpartmes = "\n".join(new_text.split("\n")[1:])
            new_mes = await bot.edit_message_text(
                text=f"{last_message_text}\n{newpartmes}",
                chat_id="@destiny_way",
                message_id=last_message_id,
            )
            db[call.from_user.username] = (new_mes.text, new_mes.id)
        else:
            text_for_channel = f"@{call.from_user.username}\n{new_text}"
            message_for_channel = await bot.send_message(
                chat_id="@destiny_way",
                text=text_for_channel,
            )
            db[call.from_user.username] = (
                text_for_channel,
                message_for_channel.id,
            )
    if str(call.message.chat.id) not in db.keys():
        db[f"{call.message.chat.id}"] = (new_text, call.message.id)
    else:
        last_message_text = db[f"{call.message.chat.id}"][0]
        last_message_id = db[f"{call.message.chat.id}"][1]
        if last_message_text.split("\n")[0] == new_text.split("\n")[0]:
            newpartmes = "\n".join(new_text.split("\n")[1:])
            new_mes = await bot.edit_message_text(
                text=f"{last_message_text}\n{newpartmes}",
                chat_id=call.message.chat.id,
                message_id=last_message_id,
            )
            await bot.delete_message(
                chat_id=call.message.chat.id,
                message_id=call.message.id,
            )
            db[f"{call.message.chat.id}"] = (new_mes.text, new_mes.id)
        else:
            db[f"{call.message.chat.id}"] = (new_text, call.message.id)


async def callback_empty_field_handler(call: types.CallbackQuery, bot: AsyncTeleBot):
    await bot.answer_callback_query(callback_query_id=call.id)


async def delete_message(message: types.Message, bot: AsyncTeleBot):
    await bot.delete_message(
        chat_id=message.chat.id,
        message_id=message.id,
    )


def bind_handlers(bot: AsyncTeleBot):
    bind_filters(
        bot=bot,
    )
    bot.register_message_handler(
        callback=command_start,
        commands=["start"],
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=command_exercise,
        commands=["exercise"],
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=command_running,
        commands=["run"],
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=command_delete,
        commands=["delete"],
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=input_exercise_name,
        state=Inputs.exercise_name,
        is_digit=False,
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=input_sets,
        state=Inputs.sets,
        is_digit=True,
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=input_reps,
        state=Inputs.reps,
        is_digit=True,
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=input_lines,
        state=Inputs.lines,
        is_digit=True,
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=input_metrs,
        state=Inputs.metrs,
        is_digit=True,
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=input_km,
        state=Inputs.km,
        is_digit=True,
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=input_metrs_km,
        state=Inputs.metrs_km,
        is_digit=True,
        pass_bot=True,
    )
    bot.register_message_handler(
        callback=delete_message,
        content_types=util.content_type_media,
        pass_bot=True,
    )
    bot.register_callback_query_handler(
        callback=time_set_callback_handler,
        func=None,
        callbackbutton=time_set.filter(),
        pass_bot=True,
    )
    bot.register_callback_query_handler(
        callback=type_running_callback_handler,
        func=None,
        callbackbutton=run_type.filter(),
        pass_bot=True,
    )
    bot.register_callback_query_handler(
        callback=callback_empty_field_handler,
        func=lambda call: call.data == EMTPY_FIELD,
        pass_bot=True,
    )
