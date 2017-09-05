using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnityEngine.Experimental.Input
{
    internal class InputDeviceProfileManager
    {
        private List<InputDeviceProfile> m_Profiles = new List<InputDeviceProfile>();
        public IEnumerable<InputDeviceProfile> profiles { get { return m_Profiles; } }

        public void RegisterProfile(InputDeviceProfile profile)
        {
            // Ignore profiles for which we already have an instance of the same type.
            for (var i = 0; i < m_Profiles.Count; ++i)
            {
                if (m_Profiles[i].GetType() == profile.GetType())
                    return;
            }

            m_Profiles.Add(profile);
        }

        /// <summary>
        /// Find the profile that should be used for a device with the given device string.
        /// </summary>
        public InputDeviceProfile FindProfileByDeviceString(string deviceString)
        {
            ////TODO: add matching for deviceNames

            // Check for exact match from device names.
            foreach (var profile in m_Profiles)
            {
                var deviceNameRegexes = profile.matchingDeviceRegexes;
                if (!string.IsNullOrEmpty(profile.neverMatchDeviceRegex)
                    && Regex.IsMatch(deviceString, profile.neverMatchDeviceRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                    continue;
                if (deviceNameRegexes != null)
                {
                    foreach (var regex in deviceNameRegexes)
                    {
                        if (Regex.IsMatch(deviceString, regex, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                            return profile;
                    }
                }
            }

            // Find a profile with a last resort match.
            foreach (var profile in m_Profiles)
            {
                var lastResortRegex = profile.lastResortDeviceRegex;
                if (!string.IsNullOrEmpty(lastResortRegex))
                {
                    if (!string.IsNullOrEmpty(profile.neverMatchDeviceRegex)
                        && Regex.IsMatch(deviceString, profile.neverMatchDeviceRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                        continue;

                    if (Regex.IsMatch(deviceString, lastResortRegex, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                        return profile;
                }
            }

            // No match. Normally means we should ignore the device.
            return null;
        }

        #if UNITY_EDITOR
        // InputDeviceManager will preserve profiles on devices and will re-create them after domain
        // reloads. At the same time, profiles will register themselves with InputSystem.RegisterProfile().
        // This will lead to duplication which we resolve here by replacing every profile that already
        // has an instance on one of the devices.
        // NOTE: InputDeviceManager already makes sure that for every profile type, it only creates one instance
        //       even if the same profile type is used by multiple devices.
        internal void ConsolidateProfilesWithThoseUsedByDevices(IEnumerable<InputDevice> devices)
        {
            var newProfileList = new List<InputDeviceProfile>();
            foreach (var profile in m_Profiles)
            {
                var profileType = profile.GetType();

                var haveFoundProfile = false;
                foreach (var device in devices)
                {
                    if (device.profile == null || device.profile.GetType() != profileType)
                        continue;

                    newProfileList.Add(device.profile);
                    haveFoundProfile = true;
                    break;
                }

                if (!haveFoundProfile)
                    newProfileList.Add(profile);
            }
            m_Profiles = newProfileList;
        }

        #endif

        // When resetting the input system, we pass on profiles from the old profile manager
        // to the new one.
        internal void StealProfilesFrom(InputDeviceProfileManager manager)
        {
            m_Profiles = manager.m_Profiles;
            manager.m_Profiles = null;
        }
    }
}
