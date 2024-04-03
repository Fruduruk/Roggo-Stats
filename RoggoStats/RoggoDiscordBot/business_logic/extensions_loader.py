import os
from interactions import Client


def load_extensions(bot: Client):
    """Automatically load all extension in the ./extensions folder"""

    # go through all folders in the directory and load the extensions from all files
    # Note: files must end in .py
    for root, dirs, files in os.walk("business_logic/extensions"):
        for file in files:
            if file.endswith(".py") and not file.startswith("_"):
                file = file.removesuffix(".py")
                path = os.path.join(root, file)
                python_import_path = path.replace("/", ".").replace("\\", ".")

                # load the extension
                bot.load_extension(python_import_path)