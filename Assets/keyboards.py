import datetime

from telebot.types import InlineKeyboardButton, InlineKeyboardMarkup

from Assets.filters import run_type, time_set

EMTPY_FIELD = "1"


class DATE:
    def NOW(self):
        week = [
            "ПОНЕДЕЛЬНИК",
            "ВТОРНИК",
            "СРЕДА",
            "ЧЕТВЕРГ",
            "ПЯТНИЦА",
            "СУББОТА",
            "ВОСКРЕСЕНЬЕ",
        ]
        now = datetime.datetime.now()
        if len(str(now.day)) == 2:
            day = now.day
        else:
            day = f"0{now.day}"
        if len(str(now.month)) == 2:
            month = now.month
        else:
            month = f"0{now.month}"
        year = now.year
        weekday = week[datetime.datetime.today().weekday()]
        return f"⚡ {day}.{month}.{year} {weekday} ⚡"


def generate_excercise_name():
    keyboard = InlineKeyboardMarkup()
    keyboard.row(
        InlineKeyboardButton(text="Напишите упражнение", callback_data=EMTPY_FIELD)
    )
    return keyboard


def generate_sets():
    keyboard = InlineKeyboardMarkup()
    keyboard.row(
        InlineKeyboardButton(text="Сколько подходов?", callback_data=EMTPY_FIELD)
    )
    return keyboard


def generate_reps():
    keyboard = InlineKeyboardMarkup()
    keyboard.row(
        InlineKeyboardButton(text=f"Сколько повторений?", callback_data=EMTPY_FIELD)
    )
    return keyboard


def generate_running_type():
    keyboard = InlineKeyboardMarkup(row_width=1)
    keyboard.row(
        InlineKeyboardButton(text=f"Какой вид бега?", callback_data=EMTPY_FIELD)
    )
    keyboard.add(
        *[
            InlineKeyboardButton(
                text="Лёгкий бег", callback_data=run_type.new(type="Лёгкий бег")
            ),
            InlineKeyboardButton(
                text="Темповой бег", callback_data=run_type.new(type="Темповой бег")
            ),
            InlineKeyboardButton(
                text="Интервалы", callback_data=run_type.new(type="Интервалы")
            ),
        ]
    )
    return keyboard


def generate_lines():
    keyboard = InlineKeyboardMarkup()
    keyboard.row(
        InlineKeyboardButton(text=f"Сколько отрезков?", callback_data=EMTPY_FIELD)
    )
    return keyboard


def generate_metrs():
    keyboard = InlineKeyboardMarkup()
    keyboard.row(
        InlineKeyboardButton(text=f"Сколько метров?", callback_data=EMTPY_FIELD)
    )
    return keyboard


def generate_km():
    keyboard = InlineKeyboardMarkup()
    keyboard.row(
        InlineKeyboardButton(text=f"Сколько километров?", callback_data=EMTPY_FIELD)
    )
    return keyboard


def generate_time_set():
    keyboard = InlineKeyboardMarkup(row_width=3)
    keyboard.row(
        InlineKeyboardButton(text="Сколько отдыхал?", callback_data=EMTPY_FIELD)
    )
    keyboard.add(
        *[
            InlineKeyboardButton(
                text=f"{i} сек", callback_data=time_set.new(time=f"{i} сек")
            )
            for i in range(30, 51, 10)
        ]
    )
    keyboard.add(
        *[
            InlineKeyboardButton(
                text=f"{i} мин", callback_data=time_set.new(time=f"{i} мин")
            )
            for i in range(1, 7)
        ]
    )
    return keyboard
