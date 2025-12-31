using System;
using System.Security;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text;
using System.ComponentModel;
using CredentialManagement;

namespace WinCord
{
    public static class TokenStorage
    {
        private const string TargetName = "WinCord_Token";
        public static void SaveToken(string token)
        {
            var cred = new Credential
            {
                Target = TargetName,
                Password = token,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer
            };
            cred.Save();
        }
        public static void DeleteToken()
        {
            var cred = new Credential {
                Target = TargetName,
                Type = CredentialType.Generic
            };
            cred.Delete();
        }
        public static string LoadToken()
        {
            var cred = new Credential 
            { 
                Target = TargetName,
                Type = CredentialType.Generic
            };

            return cred.Load() ? cred.Password : null;
        }   

    }
}
