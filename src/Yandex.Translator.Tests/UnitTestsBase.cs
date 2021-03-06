﻿using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Catharsis.Commons;
using Xunit;

namespace Yandex.Translator
{
  public abstract class UnitTestsBase<T>
  {
    protected void TestCompareTo<PROPERTY>(string property, PROPERTY lower, PROPERTY greater, Func<T> constructor = null)
    {
      Assertion.NotEmpty(property);

      constructor = constructor ?? (() => typeof(T).NewInstance().To<T>());

      var first = constructor().To<IComparable<T>>();
      var second = constructor().To<T>();

      first.Property(property, lower);
      second.Property(property, lower);

      Assert.Equal(0, first.CompareTo(second));
      second.Property(property, greater);
      Assert.True(first.CompareTo(second) < 0);
    }

    protected void TestEquality<PROPERTY>(string property, PROPERTY oldValue, PROPERTY newValue, Func<T> constructor = null)
    {
      Assertion.NotEmpty(property);

      constructor = constructor ?? (() => typeof(T).NewInstance().To<T>());
      var entity = constructor();

      Assert.False(entity.Equals(null));
      Assert.True(entity.Equals(entity));
      Assert.True(entity.Equals(constructor()));

      Assert.True(constructor().Property(property, oldValue).Equals(constructor().Property(property, oldValue)));
      Assert.False(constructor().Property(property, oldValue).Equals(constructor().Property(property, newValue)));
    }

    protected void TestHashCode<PROPERTY>(string property, PROPERTY oldValue, PROPERTY newValue, Func<T> constructor = null)
    {
      Assertion.NotEmpty(property);

      constructor = constructor ?? (() => typeof(T).NewInstance().To<T>());
      var entity = constructor();

      Assert.True(entity.GetHashCode() == entity.GetHashCode());
      Assert.True(entity.GetHashCode() == constructor().GetHashCode());

      Assert.True(constructor().Property(property, oldValue).GetHashCode() == constructor().Property(property, oldValue).GetHashCode());
      Assert.True(constructor().Property(property, oldValue).GetHashCode() != constructor().Property(property, newValue).GetHashCode());
    }

    protected void TestJson(T instance, object attributes)
    {
      Assertion.NotNull(instance);

      var json = attributes.GetType().GetProperties().Select(property =>
      {
        var value = property.GetValue(attributes, null);
        if (value is bool)
        {
          value = value.ToString().ToLowerInvariant();
        }
        else if (value is DateTime)
        {
          value = string.Format(CultureInfo.InvariantCulture, @"""{0}""", value.To<DateTime>().ISO8601());
        }
        else if (value.GetType().IsPrimitive)
        {
          value = string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }
        else if (value is string)
        {
          value = string.Format(CultureInfo.InvariantCulture, @"""{0}""", value);
        }
        else
        {
          var jsonValue = value.Json();
          value = jsonValue.Substring(0, jsonValue.Length);
        }

        return string.Format(CultureInfo.InvariantCulture, @"""{0}"":{1}", property.Name, value);
      }).Join(",");

      Assert.Equal(attributes == null ? "{}" : string.Format(CultureInfo.InvariantCulture, @"{{{0}}}", json), instance.Json());
    }

    protected void TestXml(T instance, string root, object attributes)
    {
      Assertion.NotNull(instance);
      Assertion.NotEmpty(root);

      var tags = attributes.GetType().GetProperties().Select(property =>
      {
        var value = property.GetValue(attributes, null);

        if (value is bool)
        {
          value = value.ToString().ToLowerInvariant();
        }
        else if (value is DateTime)
        {
          value = XDocument.Parse(value.To<DateTime>().ToXml()).Root.Value;
        }

        return string.Format(CultureInfo.InvariantCulture, "<{0}>{1}</{0}>", property.Name, value);
      });

      var xml = instance.ToXml();
      Assert.Contains(@"<?xml version=""1.0"" encoding=""utf-16""?>", xml);
      if (attributes == null)
      {
        Assert.Contains(string.Format(CultureInfo.InvariantCulture, @"<{0} ", root), xml);
      }
      else
      {
        Assert.Contains(string.Format(CultureInfo.InvariantCulture, @"<{0} ", root), xml);
        Assert.Contains(string.Format("</{0}>", root), xml);
        foreach (var tag in tags)
        {
          Assert.Contains(tag, xml);
        }
      }
    }
  }
}