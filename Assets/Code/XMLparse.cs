//This class reads and writes to XML files

using System.Xml;
using System.IO;

class XMLparse
{
    XmlDocument reader = new XmlDocument();
    string fileName;

    //takes the file path and if the file exists open it, if not create a new XML
    //file with the root element named the same as the file name.
    public XMLparse(string file)
    {
        if (File.Exists(file) == true)
        {
            reader.Load(file);
            fileName = file;
        }
        else
        {
            XmlNode rootNode = reader.CreateElement(file);
            reader.AppendChild(rootNode);
            reader.Save(file);
            reader.Load(file);
            fileName = file;
        }
    }

    //takes the file path and if the file exists open it, if not create a new XML
    //file with the root element named whatever is given in the rootName variable.
    public XMLparse(string file, string rootName)
    {
        if (File.Exists(file) == true)
        {
            reader.Load(file);
            fileName = file;
        }
        else
        {
            XmlNode rootNode = reader.CreateElement(rootName);
            reader.AppendChild(rootNode);
            reader.Save(file);
            reader.Load(file);
            fileName = file;
        }
    }

    //returns a value of an element with a given name in the root element.
    public string findValue(string type, string name, string value)
    {
        foreach (XmlNode xmlNode in reader.DocumentElement.ChildNodes)
        {
            if (xmlNode.LocalName == type && xmlNode.Attributes["name"].Value == name)
            {
                return xmlNode.Attributes[value].Value;
            }
        }
        return "getValue() error";
    }

    //returns a value of an element with a given index.
    public string findValue(int index, string value)
    {
        int counter = 0;
        foreach (XmlNode xmlNode in reader.DocumentElement.ChildNodes)
        {
            if (counter == index)
            {
                return xmlNode.Attributes[value].Value;
            }
            counter++;
        }
        return "getValue() error";
    }

    //returns the type of an element from a given name.
    public string findType(string name)
    {
        foreach (XmlNode xmlNode in reader.DocumentElement.ChildNodes)
        {
            if (xmlNode.Attributes["name"].Value == name)
            {
                return xmlNode.LocalName;
            }
        }
        return "findType(string) error";
    }

    //returns the type of an element from a given index.
    public string findType(int index)
    {
        int counter = 0;
        foreach (XmlNode xmlNode in reader.DocumentElement.ChildNodes)
        {
            if (counter == index)
            {
                return xmlNode.LocalName;
            }
            counter++;
        }
        return "findType(int) error";
    }

    //returns a string containing all values of a given type in the root element.
    public string listContents(string type, string value)
    {
        string tempStr = "";
        foreach (XmlNode xmlNode in reader.DocumentElement.ChildNodes)
        {
            if (xmlNode.LocalName == type)
            {
                tempStr += xmlNode.Attributes[value].Value + '\n';
            }
        }
        if (tempStr == "")
        {
            return "listContents() error";
        }
        tempStr = tempStr.TrimEnd('\n');
        return tempStr;
    }

    //returns a string containing all values of all items in the root element.
    public string listContents(string value)
    {
        string tempStr = "";
        foreach (XmlNode xmlNode in reader.DocumentElement.ChildNodes)
        {
            tempStr += xmlNode.Attributes[value].Value + '\n';
        }
        if (tempStr == "")
        {
            return "listContents() error";
        }
        tempStr = tempStr.TrimEnd('\n');
        return tempStr;
    }

    //returns the number of elements the root element has.
    public int numberOfElements()
    {
        return reader.DocumentElement.ChildNodes.Count;
    }

    //returns the number of elements with given type.
    public int numberOfElements(string type)
    {
        int tempInt = 0;
        foreach (XmlNode xmlNode in reader.DocumentElement.ChildNodes)
        {
            if (xmlNode.LocalName == type)
            {
                tempInt++;
            }
        }
        return tempInt;
    }

    //adds a new element to the root element.
    public void addElement(string type, string name, string[] dataType, string[] dataValue)
    {
        if (dataType.Length == dataValue.Length)
        {
            XmlNode node = reader.CreateElement(type);
            XmlAttribute attribute;

            attribute = reader.CreateAttribute("name");
            attribute.Value = name;
            node.Attributes.Append(attribute);

            for (int i = 0; i < dataType.Length; i++)
            {
                attribute = reader.CreateAttribute(dataType[i]);
                attribute.Value = dataValue[i];
                node.Attributes.Append(attribute);
            }

            reader.FirstChild.AppendChild(node);
            reader.Save(fileName);
        }
    }

    //converts a string to a string array
    public string[] convertToArray(string stringToConvert)
    {
        return stringToConvert.Split('\n');
    }
}