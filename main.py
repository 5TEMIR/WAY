import asyncio
import logging
import os

import telebot
from aiohttp import web
from telebot.async_telebot import AsyncTeleBot

from Assets.handlers import bind_handlers

API_TOKEN = os.environ["TOKEN"]
URL_REPL = os.environ["url_repl"]
logger = telebot.logger
telebot.logger.setLevel(logging.INFO)
bot = AsyncTeleBot(API_TOKEN)


async def handle(request):
    request_body_dict = await request.json()
    update = telebot.types.Update.de_json(request_body_dict)
    asyncio.ensure_future(bot.process_new_updates([update]))
    return web.Response()


async def shutdown(app):
    logger.info("Shutting down: removing webhook")
    await bot.remove_webhook()
    logger.info("Shutting down: closing session")
    await bot.close_session()


async def hello(request):
    logger.info("PAGE")
    return web.Response(text="BOT")


async def setup():
    logger.info("Starting up: removing old webhook")
    await bot.remove_webhook()
    logger.info("Starting up: setting webhook")
    await bot.set_webhook(url=URL_REPL + API_TOKEN)
    app = web.Application()
    app.router.add_post("/" + API_TOKEN, handle)
    app.add_routes([web.get("/", hello)])
    app.on_cleanup.append(shutdown)
    return app


if __name__ == "__main__":
    bind_handlers(bot)
    web.run_app(setup())
