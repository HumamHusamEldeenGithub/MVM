import pkgutil
import subprocess

def check_module(module_name):
    return True if pkgutil.find_loader(module_name) else False

def install_module(module_name):
    subprocess.check_call(['pip', 'install', module_name, '--disable-pip-version-check', '-q', '-q', '-q'])

def get_module_version(module_name):
    try:
        import importlib.metadata as metadata  # Python 3.8+
    except ImportError:
        import importlib_metadata as metadata  # Python 3.7 and below

    return metadata.version(module_name)

module_name = 'mediapipe'
desired_version = '10.0'

if not check_module(module_name):
    install_module(module_name)
elif get_module_version(module_name) < desired_version:
    install_module(module_name)

print(check_module(module_name))