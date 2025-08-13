using BancoLosPatitos.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Http;


namespace BancoLosPatitos.API.Controllers
{

    public class SinpeController : ApiController
    {

        [HttpGet]
        [Route("api/sinpe/obtener/{telefono}")]
        public IHttpActionResult GetSinpeTransactions(int telefono)
        {
            try
            {
                var transactions = new List<SinpeAPI>();
                string connectionString = ConfigurationManager.ConnectionStrings["BancoLosPatitosDbConnection"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM dbo.Sinpes WHERE TelefonoDestinatario = @telefono";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@telefono", telefono);
                        using (SqlDataReader reader = command.ExecuteReader())
                    {
                            while (reader.Read())
                            {
                                var transaction = new SinpeAPI
                                {
                                    IdSinpe = Convert.ToInt32(reader["IdSinpe"]),
                                    TelefonoOrigen = reader["TelefonoOrigen"].ToString(),
                                    NombreOrigen = reader["NombreOrigen"].ToString(),
                                    TelefonoDestinatario = reader["TelefonoDestinatario"].ToString(),
                                    NombreDestinatario = reader["NombreDestinatario"].ToString(),
                                    Monto = Convert.ToDecimal(reader["Monto"]),
                                    Fecha = Convert.ToDateTime(reader["FechaDeRegistro"]),
                                    Descripcion = reader["Descripcion"].ToString(),
                                    Estado = Convert.ToBoolean(reader["Estado"])
                                };
                                transactions.Add(transaction);
                            }
                        }
                    }
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPut]
        [Route("api/sinpe/sincronizar/{id}")]
        public IHttpActionResult SincronizarSinpe(int id)
        {
            try
            {
                bool actualizado = false;
                string connectionString = ConfigurationManager.ConnectionStrings["BancoLosPatitosDbConnection"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE dbo.Sinpes SET Estado = 1 WHERE IdSinpe = @id";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        int rowsAffected = command.ExecuteNonQuery();
                        actualizado = rowsAffected > 0;
                    }
                    if (actualizado)
                    {
                        return Ok(new
                        {
                            EsValido = true,
                            Mensaje = "SINPE sincronizado correctamente."
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            EsValido = false,
                            Mensaje = "No se encontró el SINPE con el Id especificado."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/sinpe/recibir")]
        public IHttpActionResult RecibirSinpe([FromBody] SinpeAPI sinpe)
        {
            try
            {
                bool insertado = false;
                string connectionString = ConfigurationManager.ConnectionStrings["BancoLosPatitosDbConnection"].ConnectionString;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO dbo.Sinpes (TelefonoOrigen, NombreOrigen, TelefonoDestinatario, NombreDestinatario, Monto, FechaDeRegistro, Descripcion, Estado) " +
                                   "VALUES (@TelefonoOrigen, @NombreOrigen, @TelefonoDestinatario, @NombreDestinatario, @Monto, @FechaDeRegistro, @Descripcion, @Estado)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TelefonoOrigen", sinpe.TelefonoOrigen);
                        command.Parameters.AddWithValue("@NombreOrigen", sinpe.NombreOrigen);
                        command.Parameters.AddWithValue("@TelefonoDestinatario", sinpe.TelefonoDestinatario);
                        command.Parameters.AddWithValue("@NombreDestinatario", sinpe.NombreDestinatario);
                        command.Parameters.AddWithValue("@Monto", sinpe.Monto);
                        command.Parameters.AddWithValue("@FechaDeRegistro", DateTime.Now);
                        command.Parameters.AddWithValue("@Descripcion", sinpe.Descripcion);
                        command.Parameters.AddWithValue("@Estado", false);

                        int rowsAffected = command.ExecuteNonQuery();
                        insertado = rowsAffected > 0;
                    }
                }

                if (insertado)
                {
                    return Ok(new
                    {
                        EsValido = true,
                        Mensaje = "SINPE registrado correctamente."
                    });
                }
                else
                {
                    return Ok(new
                    {
                        EsValido = false,
                        Mensaje = "No se pudo registrar el SINPE."
                    });
                }
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    EsValido = false,
                    Mensaje = "Ocurrió un error al registrar el SINPE: " + ex.Message
                });
            }
        }
    }
}