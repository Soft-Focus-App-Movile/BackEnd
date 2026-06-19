namespace AgoraIO.Media
{
    /// <summary>
    /// Vendored from AgoraIO/Tools (RtcTokenBuilder2), trimmed to the user-account builders this
    /// backend uses. Generates RTC tokens (AccessToken2, version "007").
    /// </summary>
    public class RtcTokenBuilder2
    {
        public enum Role
        {
            /// RECOMMENDED for a voice/video call or live broadcast when co-host authentication is not required.
            RolePublisher = 1,

            /// Only for scenarios that require co-host authentication.
            RoleSubscriber = 2
        }

        /// <summary>
        /// Build the RTC token with a string user account.
        /// </summary>
        /// <param name="appId">The App ID issued to you by Agora.</param>
        /// <param name="appCertificate">The App Certificate of your Agora project.</param>
        /// <param name="channelName">Unique channel name for the AgoraRTC session.</param>
        /// <param name="account">The user's account (max 255 bytes).</param>
        /// <param name="role">Publisher (broadcaster/host) or Subscriber (audience).</param>
        /// <param name="tokenExpire">Seconds from now until the token expires.</param>
        /// <param name="privilegeExpire">Seconds from now until the privilege expires.</param>
        public static string buildTokenWithUserAccount(string appId, string appCertificate, string channelName, string account, Role role, uint tokenExpire, uint privilegeExpire)
        {
            AccessToken2 accessToken = new AccessToken2(appId, appCertificate, tokenExpire);
            AccessToken2.Service serviceRtc = new AccessToken2.ServiceRtc(channelName, account);

            serviceRtc.addPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PRIVILEGE_JOIN_CHANNEL, privilegeExpire);
            if (Role.RolePublisher == role)
            {
                serviceRtc.addPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PRIVILEGE_PUBLISH_AUDIO_STREAM, privilegeExpire);
                serviceRtc.addPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PRIVILEGE_PUBLISH_VIDEO_STREAM, privilegeExpire);
                serviceRtc.addPrivilegeRtc(AccessToken2.PrivilegeRtcEnum.PRIVILEGE_PUBLISH_DATA_STREAM, privilegeExpire);
            }
            accessToken.addService(serviceRtc);

            return accessToken.build();
        }

        /// <summary>
        /// Build the RTC token with a numeric uid. Pass uid = 0 to skip uid authentication.
        /// </summary>
        public static string buildTokenWithUid(string appId, string appCertificate, string channelName, uint uid, Role role, uint tokenExpire, uint privilegeExpire)
        {
            return buildTokenWithUserAccount(appId, appCertificate, channelName, AccessToken2.getUidStr(uid), role, tokenExpire, privilegeExpire);
        }
    }
}
