using Dapper;
using MySql.Data.MySqlClient;
namespace Kps.Integration.Api.Services;

public abstract class MySqlBaseService
{
    private string _connectionString;

    protected MySqlBaseService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public MySqlConnection GetMySqlConnection()
    {
        try
        {
            return new MySqlConnection(_connectionString);
        }
        catch (Exception ex)
        {
            throw ex;
        } 
    }
}