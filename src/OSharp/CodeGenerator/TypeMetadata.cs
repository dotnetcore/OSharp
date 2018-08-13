// -----------------------------------------------------------------------
//  <copyright file="EntityMetadata.cs" company="OSharp��Դ�Ŷ�">
//      Copyright (c) 2014-2018 OSharp. All rights reserved.
//  </copyright>
//  <site>http://www.osharp.org</site>
//  <last-editor>������</last-editor>
//  <last-date>2018-08-06 12:25</last-date>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

using OSharp.Reflection;


namespace OSharp.CodeGenerator
{
    /// <summary>
    /// ����Ԫ����
    /// </summary>
    public class TypeMetadata
    {
        /// <summary>
        /// ��ʼ��һ��<see cref="TypeMetadata"/>���͵���ʵ��
        /// </summary>
        public TypeMetadata()
        { }

        /// <summary>
        /// ��ʼ��һ��<see cref="TypeMetadata"/>���͵���ʵ��
        /// </summary>
        public TypeMetadata(Type type)
        {
            if (type == null)
            {
                return;
            }
            
            Name = type.Name;
            FullName = type.FullName;
            Namespace = type.Namespace;
            Display = type.GetDescription();
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties.Where(m => !m.HasAttribute<IgnoreGenPropertyAttribute>()))
            {
                if (PropertyMetadatas == null)
                {
                    PropertyMetadatas = new List<PropertyMetadata>();
                }
                PropertyMetadatas.Add(new PropertyMetadata(property));
            }
        }
        
        /// <summary>
        /// ��ȡ������ ������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ��ȡ������ ����ȫ��
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// ��ȡ������ �����ռ�
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// ��ȡ������ ������ʾ��
        /// </summary>
        public string Display { get; set; }

        /// <summary>
        /// ��ȡ������ ����Ԫ���ݼ���
        /// </summary>
        public IList<PropertyMetadata> PropertyMetadatas { get; set; }
    }
}