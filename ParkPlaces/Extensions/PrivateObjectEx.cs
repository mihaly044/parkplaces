using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParkPlaces.Map_shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ParkPlaces.Extensions
{
    public static class PrivateObjectEx
    {
        public static bool TryFindField<T>
            (this PrivateObject po, string name, out T value)
        {
            return po.TryFindField<T>
                (name, System.Reflection.BindingFlags.Default, out value);
        }

        public static bool TryFindField<T>(this PrivateObject po,
            string name, System.Reflection.BindingFlags flags, out T value)
        {
            Type t = po.RealType;
            var found = false;
            value = default(T);
            do
            {
                var field = t.GetFields(flags)
                    .FirstOrDefault(f => f.Name == name);
                if (field != default(System.Reflection.FieldInfo))
                {
                    value = (T) field.GetValue(po.Target);
                    found = true;
                }
                else
                {
                    t = t.BaseType;
                }
            } while (!found && t != null);

            return found;
        }
    }
}