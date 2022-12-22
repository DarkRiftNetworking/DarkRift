#!/usr/bin/env python
import os
import sys

"""
Get version from .props file.
"""
def get_version_number():
    dirname = os.path.dirname(__file__)
    props = os.path.join(dirname, '../.props')

    import xml.etree.ElementTree as ET
    xmldoc = ET.parse(props)
    root = xmldoc.getroot()

    return root.find("PropertyGroup").find("Version").text

print(get_version_number())
