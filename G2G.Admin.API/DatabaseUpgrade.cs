using Microsoft.Data.Sqlite;

namespace G2G.Admin.API;

/// <summary>
/// 数据库升级工具 - 用于处理旧版本数据库结构升级
/// </summary>
public static class DatabaseUpgrade
{
    /// <summary>
    /// 执行数据库升级
    /// </summary>
    public static void Upgrade(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        // 检查 Versions 表是否有 OriginalFileName 列
        if (!ColumnExists(connection, "Versions", "OriginalFileName"))
        {
            Console.WriteLine("📦 正在升级 Versions 表...");
            var command = connection.CreateCommand();
            command.CommandText = @"
                ALTER TABLE Versions 
                ADD COLUMN OriginalFileName TEXT NOT NULL DEFAULT '';
            ";
            command.ExecuteNonQuery();
            Console.WriteLine("✅ 添加 OriginalFileName 列到 Versions 表");
        }

        // 未来升级可以添加到这里
        // if (!ColumnExists(connection, "TableName", "NewColumn")) { ... }
    }

    /// <summary>
    /// 检查表中是否存在指定列
    /// </summary>
    private static bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
    {
        var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";
        
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (reader.GetString(1) == columnName)
            {
                return true;
            }
        }
        return false;
    }
}
