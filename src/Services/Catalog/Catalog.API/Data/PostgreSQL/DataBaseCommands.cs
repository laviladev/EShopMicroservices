using System.Collections;
using System.Reflection;
using Npgsql;

namespace Catalog.API.Data.PostgreSQL
{
    public class DataBaseCommands
    {
        private readonly INpgsqlConnectionProvider _connectionProvider;

        public DataBaseCommands(INpgsqlConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task<bool> WriteCommand(string sqlCommand, Dictionary<string, object> parameters)
        {
            using var connection = _connectionProvider.GetConnection();
            connection.Open();

            using var command = new NpgsqlCommand(sqlCommand, connection);
            // Agregar los parámetros al comando
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                Console.WriteLine($"{param.Key} = {param.Value}");
            }

            // Ejecutar el comando
            return await command.ExecuteNonQueryAsync() > 0;
        }
        public async Task<List<T>> ReadQuery<T>(string sqlCommand, Dictionary<string, object> parameters, List<string>? propertiesToLoad = null, string? relationalTableName = null) where T : new()
        {
            using var connection = _connectionProvider.GetConnection();
            connection.Open();
            List<T> result = [];

            Console.WriteLine($"Executing: {sqlCommand}");
            using var command = new NpgsqlCommand(sqlCommand, connection);
            // Agregar los parámetros al comando
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                Console.WriteLine($"{param.Key} = {param.Value}");
            }

            // Ejecutar el comando
            using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(await MapReaderToObject<T>(reader, propertiesToLoad, relationalTableName));
                }
            }
            return result;
        }

    private async Task<T> MapReaderToObject<T>(NpgsqlDataReader reader, List<string>? propertiesToLoad = null, string? relationalTableName = null) where T : new()
    {
        T obj = new T();
        Type type = typeof(T);
        propertiesToLoad ??= [];
        using var connection = _connectionProvider.GetConnection();
        connection.Open();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            string columnName = reader.GetName(i);
            object? value = reader.IsDBNull(i) ? null : reader.GetValue(i);

            PropertyInfo? property = type.GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
        }

        // Handle many-to-many relationships selectively
        var manyToManyProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in manyToManyProperties)
        {
            if (propertiesToLoad.Contains(property.Name) && property.PropertyType.IsGenericType && relationalTableName != null)
            {
                Type itemType = property.PropertyType.GetGenericArguments()[0];
                var relatedItems = await GetRelatedItems(obj, property, itemType, relationalTableName);
                property.SetValue(obj, relatedItems);
            }
        }

        return obj;
    }

    private async Task<object> GetRelatedItems(object parentObj, PropertyInfo property, Type itemType, string relationalTableName)
    {
        using var connection = _connectionProvider.GetConnection();
        connection.Open();
        string parentIdName = "Id";
        PropertyInfo parentIdProperty = parentObj.GetType().GetProperty(parentIdName) ??
            throw new InvalidOperationException($"Parent object does not have a property named '{parentIdName}'");
        
        object parentId = parentIdProperty.GetValue(parentObj) ?? throw new InvalidOperationException("Parent ID cannot be null");
        
        string relatedTableName = property.Name;
        string joinTableName = $"{relatedTableName}_{relationalTableName}";
        string parentIdColumnName = $"{parentObj.GetType().Name}_id";
        string relatedIdColumnName = $"{itemType.Name}_id";

        string sql = $@"
            SELECT t.*
            FROM {relatedTableName} t
            JOIN {joinTableName} jt ON t.id = jt.{relatedIdColumnName}
            WHERE jt.{parentIdColumnName} = @ParentId";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@ParentId", parentId);

        var relatedItems = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));

        using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var item = Activator.CreateInstance(itemType);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string columnName = reader.GetName(i);
                    object? value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    PropertyInfo? itemProperty = itemType.GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (itemProperty != null && itemProperty.CanWrite)
                    {
                        itemProperty.SetValue(item, value);
                    }
                }
                relatedItems.Add(item);
            }
        }

        return relatedItems;
    }

        public async Task<int> ReadQueryExecuteScalar(string sqlCommand, Dictionary<string, object> parameters)
        {
            using var connection = _connectionProvider.GetConnection();
            connection.Open();

            using var command = new NpgsqlCommand(sqlCommand, connection);
            // Agregar los parámetros al comando
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                Console.WriteLine($"{param.Key} = {param.Value}");
            }
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
    }
}