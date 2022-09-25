using NetScriptFramework.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FixOfSteel {
	public sealed class Settings {
        [ConfigValue("UseSkill", "Whether or not heavy armor skill is included in the calculations")]
        public bool UseSkill { get; set; } = true;
        [ConfigValue("UsePerks", "Whether or not Juggernaut perks are included in the calculations")]
        public bool UsePerks { get; set; } = true;
        [ConfigValue("UseQuality", "Whether or not smithing quality is included in the calculations")]
        public bool UseQuality { get; set; } = true;
        internal void Load() {
            ConfigFile.LoadFrom(this, "FixOfSteel", true);
        }
    }
}
