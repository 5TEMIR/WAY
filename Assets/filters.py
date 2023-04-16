from telebot import types
from telebot.async_telebot import AsyncTeleBot
from telebot.asyncio_filters import AdvancedCustomFilter, IsDigitFilter, StateFilter
from telebot.callback_data import CallbackData, CallbackDataFilter

time_set = CallbackData("time", prefix="time_set")
run_type = CallbackData("type", prefix="run_type")


class CallbackFilter(AdvancedCustomFilter):
    key = "callbackbutton"

    async def check(self, call: types.CallbackQuery, config: CallbackDataFilter):
        return config.check(query=call)


def bind_filters(bot: AsyncTeleBot):
    bot.add_custom_filter(CallbackFilter())
    bot.add_custom_filter(StateFilter(bot))
    bot.add_custom_filter(IsDigitFilter())
