using System;
using System.IO;
using System.Linq;

namespace PSBootstrap.Shared.Value_object;

public class XmlPath(string xmlPath) : CheckFile(xmlPath, "XML", false, [".xml"]);