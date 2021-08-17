using System.Collections.Generic;

namespace EasyCraft.Base.User
{
    public class Permission
    {
        public static Dictionary<PermissionId, UserType> PermissionList = new()
        {
            { PermissionId.Nothing, UserType.Nobody },
            { PermissionId.ChangeCore, UserType.Registered },
            { PermissionId.ChangeStarter, UserType.Registered }
        };

        public static void LoadPermissions()
        {
            var reader = Database.Database.CreateCommand("SELECT id, type FROM permissions").ExecuteReader();
            while (reader.Read())
            {
                PermissionList[(PermissionId)reader.GetInt32(0)] = (UserType)reader.GetInt32(1);
            }
        }

        public static bool CheckPermission(PermissionId pid, UserType type)
        {
            return PermissionList[pid] >= type;
        }
    }

    public enum PermissionId
    {
        Nothing,
        ChangeCore,
        ChangeStarter
    }
}