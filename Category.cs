using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace Home_Accounting
{
    class Category
    {
        const char Slash = '/';

        public bool Exists(string fullName)
        {
            return GetId(fullName).HasValue;
        }
        public Nullable<int> GetId(string fullName)
        {
            if (fullName is null) return null;
            int parentId = 0;
            return getId(parentId, fullName.Split('/'), 0);
        }
        public int CreateMissing(string fullName)
        {
            return createMissing(0, fullName.Split('/'), 0, true);
        }
        public int createMissing(int parentId, string[] nameParts, int partsIndex, bool checkExisting)
        {
            if (partsIndex >= nameParts.Length) throw new ArgumentOutOfRangeException("partsIndex");

            int? id = null;
            if (checkExisting) id = getId(parentId, nameParts[partsIndex]);

            if (!id.HasValue)
            {
                Create(parentId, nameParts[partsIndex]);
                checkExisting = false; // no need to search in deeper levels
            }

            id = getId(parentId, nameParts[partsIndex]);

            if (!id.HasValue) throw new Exception(string.Format("Category should exist {0}-{1}", parentId, nameParts[partsIndex]));

            if (partsIndex == nameParts.Length - 1)
            {
                return id.Value;
            }
            else
            {
                return createMissing(id.Value, nameParts, partsIndex + 1, checkExisting);
            }
        }
        private void Create(int parentId, string name)
        {
            cmdCreate.Parameters["name"].Value = name;
            cmdCreate.Parameters["parentId"].Value = parentId;
            cmdCreate.ExecuteNonQuery();
        }

        public string GetFullName(int id)
        {
            string fullName = null;
            for (;;) {
                cmdGet.Parameters["Id"].Value = id;
                using (OleDbDataReader reader = cmdGet.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        throw new Exception(string.Format("No Category row for id:{0}, fullName:{1} so far", id, fullName));
                    }
                    string name = reader.GetString(1);
                    if (fullName == null)
                    {
                        fullName = name;
                    }
                    else
                    {
                        fullName = name + Slash + fullName;
                    }
                    int parentId = reader.GetInt32(0);
                    if (parentId == 0) break;
                    id = parentId;
                }
            }
            return fullName;
        }

        private Nullable<int> getId(int parentId, string name)
        {
            cmdFind.Parameters["name"].Value = name;
            cmdFind.Parameters["parentId"].Value = parentId;
            object res = cmdFind.ExecuteScalar();
            return res as int?;
        }
        private Nullable<int> getId(int parentId, string[] nameParts, int partsIndex)
        {
            if (partsIndex < nameParts.Length)
            {
                int? id = getId(parentId, nameParts[partsIndex]);
                if (!id.HasValue || partsIndex >= nameParts.GetUpperBound(0))
                {
                    return id;
                } else
                {
                    return getId(id.Value, nameParts, partsIndex + 1);
                }
            } else
            {
                return null;
            }
        }
        private OleDbCommand cmdFind;
        private OleDbCommand cmdGet;
        private OleDbCommand cmdCreate;

        public Category()
        {
            cmdFind = DataUtil.CreateCommand("select Id from PurchaseCategory where Name = @name and ParentID = @parentId");
            cmdFind.Parameters.Add("name", OleDbType.VarWChar);
            cmdFind.Parameters.Add("parentId", OleDbType.Integer);

            cmdGet = DataUtil.CreateCommand("select ParentId, Name from PurchaseCategory where ID = @Id");
            cmdGet.Parameters.Add("Id", OleDbType.Integer);

            cmdCreate = DataUtil.CreateCommand("insert into PurchaseCategory(Name, ParentID) values (@name, @parentId)");
            cmdCreate.Parameters.Add("name", OleDbType.VarWChar);
            cmdCreate.Parameters.Add("parentId", OleDbType.Integer);
        }
    }
}
