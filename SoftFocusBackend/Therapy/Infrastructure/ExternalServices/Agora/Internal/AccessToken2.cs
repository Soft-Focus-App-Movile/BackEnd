using System;
using System.Collections.Generic;

namespace AgoraIO.Media
{
    /// <summary>
    /// Vendored from AgoraIO/Tools (AccessToken2, version "007"), trimmed to the RTC service which
    /// is the only one this backend issues. The wire format and signing algorithm are unchanged, so
    /// tokens produced here are byte-compatible with Agora's reference builders.
    /// </summary>
    public class AccessToken2
    {
        private static readonly string VERSION = "007";

        public string _appCert = "";
        public string _appId = "";
        public uint _expire;
        public uint _issueTs;
        public uint _salt;
        public Dictionary<ushort, Service> _services = new Dictionary<ushort, Service>();

        public AccessToken2()
        {
        }

        public AccessToken2(string appId, string appCert, uint expire)
        {
            _appCert = appCert;
            _appId = appId;
            _expire = expire;
            _issueTs = (uint)Utils.getTimestamp();
            _salt = (uint)Utils.randomInt();
        }

        public void addService(Service service)
        {
            _services.Add((ushort)service.getServiceType(), service);
        }

        public static short SERVICE_TYPE_RTC = 1;

        public static string getUidStr(uint uid)
        {
            if (uid == 0)
            {
                return "";
            }
            return (uid & 0xFFFFFFFFL).ToString();
        }

        public string getVersion()
        {
            return VERSION;
        }

        public byte[] getSign()
        {
            byte[] signing = DynamicKeyUtil.encodeHMAC(BitConverter.GetBytes(_issueTs), _appCert.getBytes(), "SHA256");
            return DynamicKeyUtil.encodeHMAC(BitConverter.GetBytes(_salt), signing, "SHA256");
        }

        public string build()
        {
            if (!Utils.isUUID(_appId) || !Utils.isUUID(_appCert))
            {
                return "";
            }

            ByteBuf buf = new ByteBuf().put(_appId.getBytes()).put((uint)_issueTs).put(_expire).put((uint)_salt).put((ushort)_services.Count);
            byte[] signing = getSign();

            foreach (var it in _services)
            {
                it.Value.pack(buf);
            }

            byte[] signature = DynamicKeyUtil.encodeHMAC(signing, buf.asBytes(), "SHA256");

            ByteBuf bufferContent = new ByteBuf();
            bufferContent.put(signature);
            bufferContent.copy(buf.asBytes());

            return getVersion() + Utils.base64Encode(Utils.compress(bufferContent.asBytes()));
        }

        public enum PrivilegeRtcEnum
        {
            PRIVILEGE_JOIN_CHANNEL = 1,
            PRIVILEGE_PUBLISH_AUDIO_STREAM = 2,
            PRIVILEGE_PUBLISH_VIDEO_STREAM = 3,
            PRIVILEGE_PUBLISH_DATA_STREAM = 4
        }

        public class Service
        {
            private short _type;
            private Dictionary<ushort, uint> _privileges = new Dictionary<ushort, uint>();

            public Service()
            {
            }

            public Service(short serviceType)
            {
                _type = serviceType;
            }

            public void addPrivilegeRtc(PrivilegeRtcEnum privilege, uint expire)
            {
                _privileges.Add((ushort)privilege, expire);
            }

            public Dictionary<ushort, uint> getPrivileges()
            {
                return _privileges;
            }

            public short getServiceType()
            {
                return _type;
            }

            public void setServiceType(short type)
            {
                _type = type;
            }

            public virtual ByteBuf pack(ByteBuf buf)
            {
                return buf.put((ushort)_type).putIntMap(_privileges);
            }
        }

        public class ServiceRtc : Service
        {
            public string _channelName;
            public string _uid;

            public ServiceRtc()
            {
                setServiceType(SERVICE_TYPE_RTC);
            }

            public ServiceRtc(string channelName, string uid)
            {
                setServiceType(SERVICE_TYPE_RTC);
                _channelName = channelName;
                _uid = uid;
            }

            public string getChannelName()
            {
                return _channelName;
            }

            public string getUid()
            {
                return _uid;
            }

            public override ByteBuf pack(ByteBuf buf)
            {
                return base.pack(buf).put(_channelName.getBytes()).put(_uid.getBytes());
            }
        }
    }
}
