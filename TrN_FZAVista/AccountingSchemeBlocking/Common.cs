// Decompiled with JetBrains decompiler
// Type: SchamatyKsiegowe.Common
// Assembly: AccountingSchemeBlocking, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3A48C3EA-DBA9-4782-8D5B-DADA3ED5216D
// Assembly location: D:\Documents\DeHeusRPA\Archiwum\Hydry\AccountingSchemeBlocking.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SchamatyKsiegowe
{
  internal static class Common
  {
    public static string ToString(this IEnumerable source, string separator)
    {
      if (source == null)
        throw new ArgumentException("Parameter source can not be null.");
      if (string.IsNullOrEmpty(separator))
        throw new ArgumentException("Parameter separator can not be null or empty.");
      string[] array = source.Cast<object>().Where<object>((Func<object, bool>) (n => n != null)).Select<object, string>((Func<object, string>) (n => n.ToString())).ToArray<string>();
      return string.Join(separator, array);
    }

    public static string ToString<T>(this IEnumerable<T> source, string separator)
    {
      if (source == null)
        throw new ArgumentException("Parameter source can not be null.");
      if (string.IsNullOrEmpty(separator))
        throw new ArgumentException("Parameter separator can not be null or empty.");
      string[] array = source.Where<T>((Func<T, bool>) (n => (object) n != null)).Select<T, string>((Func<T, string>) (n => n.ToString())).ToArray<string>();
      return string.Join(separator, array);
    }
  }
}
