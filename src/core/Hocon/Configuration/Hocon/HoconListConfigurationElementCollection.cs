﻿//-----------------------------------------------------------------------
// <copyright file="HoconListConfigurationElementCollection.cs" company="Hocon Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/hocon>
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;

namespace Akka.Configuration.Hocon
{
    /// <summary>
    /// This class represents a list of custom HOCON (Human-Optimized Config Object Notation)
    /// nodes within a configuration file.
    /// <code>
    /// <![CDATA[
    /// <?xml version="1.0" encoding="utf-8" ?>
    /// <configuration>
    ///   <configSections>
    ///     <section name="akka" type="AkkaConfiguration.Hocon.AkkaConfigurationSection, Akka" />
    ///   </configSections>
    ///   <akka>
    ///     <hocon-list>
    ///       <hocon>
    ///       ...
    ///       </hocon>
    ///       <hocon>
    ///       ...
    ///       </hocon>
    ///       <hocon>
    ///       ...
    ///       </hocon>
    ///     </hocon-list>
    ///   </akka>
    /// </configuration>
    /// ]]>
    /// </code>
    /// </summary>
    [ConfigurationCollection(typeof(HoconConfigurationElement), AddItemName = "hocon", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class HoconListConfigurationElementCollection : ConfigurationElementCollection
    {
        public HoconConfigurationElement this[int index]
        {
            get { return (HoconConfigurationElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(HoconConfigurationElement serviceConfig)
        {
            BaseAdd(serviceConfig);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new HoconConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((HoconConfigurationElement)element).Content;
        }

        public void Remove(HoconConfigurationElement serviceConfig)
        {
            int i = BaseIndexOf(serviceConfig);
            if (i >= 0)
            {
                BaseRemoveAt(i);
            }
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}
