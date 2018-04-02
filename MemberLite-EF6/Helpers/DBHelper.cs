using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

public class DBHelper
{
    public static string HandleDBNull(object Value, bool IsNumeric = false)
    {
        if (Value != DBNull.Value)
        {
            return Value.ToString();
        }
        else
        {
            if (IsNumeric)
            {
                return "0";
            }
            else
            { return ""; }
        }
    }

    public static string HandleEFException(DbEntityValidationException Exception, bool Log = true)
    {
        foreach (DbEntityValidationResult item in Exception.EntityValidationErrors)
        {
            // Get entry
            var entry = item.Entry;
            string entityTypeName = entry.Entity.GetType().Name;

            // Display or log error messages
            foreach (DbValidationError subItem in item.ValidationErrors)
            {
                string message = string.Format("Error '{0}' occurred in {1} at {2}",
                         subItem.ErrorMessage, entityTypeName, subItem.PropertyName);
                if (Log) CustomErrorLogger.Log(message, "500");

                // Rollback changes
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.State = EntityState.Detached;
                        break;
                    case EntityState.Modified:
                        entry.CurrentValues.SetValues(entry.OriginalValues);
                        entry.State = EntityState.Unchanged;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Unchanged;
                        break;
                }

                return message;
            }
        }

        return "";
    }
}