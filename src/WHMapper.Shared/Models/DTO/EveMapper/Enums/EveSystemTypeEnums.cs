using System.ComponentModel;

namespace WHMapper.Shared.Models.DTO.EveMapper.Enums
{
    public enum EveSystemType : int
    {
        HS = 7,
        LS = 8,
        NS = 9,
        [Description("T")]
        Pochven = 25,
        C1 = 1,
        C2 = 2,
        C3 = 3,
        C4 = 4,
        C5 = 5,
        C6 = 6,
        C13 = 13,
        C14 = 14,
        C15 = 15,
        C16 = 16,
        C17 = 17,
        C18 = 18,
        Thera = 12,
        None = -1
    }

    public static class EveSystemTypeExtensions
    {
        public static string ToDescriptionString(this EveSystemType This)
        {
            var type = This.GetType();

            string? name = Enum.GetName(type, This);
            if (name == null)
                return string.Empty;
            else
            {
                var members = type.GetMembers();
                var member = members.Where(w => w.Name == name).FirstOrDefault();

                var attribute = member != null ? member.GetCustomAttributes(true).Where(w => w.GetType() == typeof(DescriptionAttribute)).FirstOrDefault() as DescriptionAttribute : null;

                return attribute != null ? attribute.Description : name;
            }
        }
    }
}

