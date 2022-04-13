#!/usr/bin/env python
import sys

"""
Get version from .props file.
"""
def get_version_number():
    import xml.etree.ElementTree as ET
    xmldoc = ET.parse("./.props")
    root = xmldoc.getroot()

    return root.find("PropertyGroup").find("Version").text

print(get_version_number())
