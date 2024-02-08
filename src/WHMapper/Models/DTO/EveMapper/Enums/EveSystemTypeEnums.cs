using System;
using System.ComponentModel;
using System.Reflection;

namespace WHMapper.Models.DTO.EveMapper.Enums
{
    public enum EveSystemType
    {
        HS,
        LS,
        NS,
        [Description("T")]
        Pochven,
        C1,
        C2,
        C3,
        C4,
        C5,
        C6,
        C13,
        C14,
        C15,
        C16,
        C17,
        C18,
        Thera,
        K162,
        C123,
        C45,
        None
    }

    public static class EveSystemTypeExtensions
    {
        public static string ToDescriptionString(this EveSystemType This)
        {
            Type type = This.GetType();
            
            string? name = Enum.GetName(type, This);
            if (name == null)
                return string.Empty;
            else
            {
                var members=type.GetMembers();
                MemberInfo? member = members.Where(w => w.Name == name).FirstOrDefault();

                DescriptionAttribute? attribute = ((member != null) ? member.GetCustomAttributes(true).Where(w => w.GetType() == typeof(DescriptionAttribute)).FirstOrDefault() as DescriptionAttribute : null);

                return ((attribute != null) ? attribute.Description : name);
            }
        }
    }
}

