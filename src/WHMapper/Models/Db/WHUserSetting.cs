using System.ComponentModel.DataAnnotations;

namespace WHMapper.Models.Db
{
    public class WHUserSetting
    {
        // Default constants
        public const string DEFAULT_KEY_LINK = "KeyL";
        public const string DEFAULT_KEY_DELETE = "Delete";
        public const string DEFAULT_KEY_INCREMENT_EXTENSION = "NumpadAdd";
        public const string DEFAULT_KEY_DECREMENT_EXTENSION = "NumpadSubtract";
        public const string DEFAULT_KEY_INCREMENT_EXTENSION_ALT = "ArrowUp";
        public const string DEFAULT_KEY_DECREMENT_EXTENSION_ALT = "ArrowDown";
        public const bool DEFAULT_ZOOM_ENABLED = true;
        public const bool DEFAULT_ZOOM_INVERSE = false;
        public const bool DEFAULT_ALLOW_MULTI_SELECTION = true;
        public const bool DEFAULT_LINK_SNAPPING = false;
        public const double DEFAULT_NODE_SPACING = 30;
        public const double DEFAULT_DRAG_THRESHOLD = 5;

        [Key]
        public int Id { get; set; }

        [Required]
        public int EveCharacterId { get; set; }

        // Keyboard settings
        [Required, StringLength(50)]
        public string KeyLink { get; set; } = DEFAULT_KEY_LINK;

        [Required, StringLength(50)]
        public string KeyDelete { get; set; } = DEFAULT_KEY_DELETE;

        [Required, StringLength(50)]
        public string KeyIncrementExtension { get; set; } = DEFAULT_KEY_INCREMENT_EXTENSION;

        [Required, StringLength(50)]
        public string KeyDecrementExtension { get; set; } = DEFAULT_KEY_DECREMENT_EXTENSION;

        [Required, StringLength(50)]
        public string KeyIncrementExtensionAlt { get; set; } = DEFAULT_KEY_INCREMENT_EXTENSION_ALT;

        [Required, StringLength(50)]
        public string KeyDecrementExtensionAlt { get; set; } = DEFAULT_KEY_DECREMENT_EXTENSION_ALT;

        // Mouse / Zoom settings
        public bool ZoomEnabled { get; set; } = DEFAULT_ZOOM_ENABLED;
        public bool ZoomInverse { get; set; } = DEFAULT_ZOOM_INVERSE;
        public bool AllowMultiSelection { get; set; } = DEFAULT_ALLOW_MULTI_SELECTION;
        public bool LinkSnapping { get; set; } = DEFAULT_LINK_SNAPPING;

        // Map settings
        public double NodeSpacing { get; set; } = DEFAULT_NODE_SPACING;
        public double DragThreshold { get; set; } = DEFAULT_DRAG_THRESHOLD;

        [Obsolete("EF Requires it")]
        protected WHUserSetting() { }

        public WHUserSetting(int eveCharacterId)
        {
            EveCharacterId = eveCharacterId;
        }

        public static WHUserSetting CreateDefault(int eveCharacterId)
        {
            return new WHUserSetting(eveCharacterId);
        }
    }
}
