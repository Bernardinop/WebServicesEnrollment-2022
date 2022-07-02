using WebServicesEnrollment.Models;
using System.Data.SqlClient;
using System.Data;
using System.Text.Json;
using Serilog;

namespace WebServicesEnrollment.Service
{
    public class EnrollmentService : IEnrollmentService
    {
        private SqlConnection connection = new SqlConnection("Server=localhost;Database=kalum_test;User Id=sa;Password=Inicio.2022;");
        private AppLog AppLog = new AppLog();
        public EnrollmentService()
        {

        }
        public EnrollmentResponse EnrollmentProcess(EnrollmentRequest request)
        {
            AppLog.ResponseTime = Convert.ToInt16(DateTime.Now.ToString("fff"));
            AppLog.DateTime = DateTime.Now.ToString("yyyy-mm-dd HH:mm:ss");
            EnrollmentResponse respuesta = null;
            Aspirante aspirante = buscarAspirante(request.NoExpediente);
            if (aspirante == null)
            {
                respuesta = new EnrollmentResponse() { Codigo = 204, Respuesta = "No existen registros" };
                ImprimirLog(204, $"No existen registros para el numero de expediente {request.NoExpediente}", "Information");
            }
            else
            {
                respuesta = EjecutarProcedimiento(request);
            }
            return respuesta;
        }

        private EnrollmentResponse EjecutarProcedimiento(EnrollmentRequest request)
        {
            EnrollmentResponse response = null;
            SqlCommand cmd = new SqlCommand("sp_EnrollmentProcess", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@NoExpediente", request.@NoExpediente));
            cmd.Parameters.Add(new SqlParameter("@Ciclo", request.Ciclo));
            cmd.Parameters.Add(new SqlParameter("@MesInicioPago", request.MesInicioPago));
            cmd.Parameters.Add(new SqlParameter("@CarreraId", request.CarreraId));
            SqlDataReader reader = null;
            try
            {
                connection.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    response = new EnrollmentResponse() { Respuesta = reader.GetValue(0).ToString(), Carne = reader.GetValue(1).ToString() };
                    if (reader.GetValue(0).ToString().Equals("Transaction Success"))
                    {
                        response.Codigo = 201;
                        ImprimirLog(201, reader.GetValue(0).ToString(), "Information");
                    }
                    else if (reader.GetValue(0).ToString().Equals("Transaction Error"))
                    {
                        response.Codigo = 503;
                        ImprimirLog(503, reader.GetValue(0).ToString(), "Error");
                    }
                    else
                    {
                        response.Codigo = 503;
                        ImprimirLog(503, "Error al momento de llamar al procedimiento almacenado", "Error");
                    }
                }
                reader.Close();
                connection.Close();
            }
            catch (Exception e)
            {
                response = new EnrollmentResponse() { Codigo = 503, Respuesta = "Error al momento de llamar al procedimiento almacenado", Carne = "0" };
                ImprimirLog(503, "Error al momento de llamar al procedimiento almacenado", "Error");
            }
            finally
            {
                connection.Close();
            }
            return response;
        }

        private void ImprimirLog(int responseCode, string message, string typeLog)
        {
            AppLog.ResponseCode = responseCode;
            AppLog.Message = message;
            AppLog.ResponseTime = Convert.ToInt16(DateTime.Now.ToString("fff")) - AppLog.ResponseTime;
            if (typeLog.Equals("Information"))
            {
                AppLog.Level = 20;
                Log.Information(JsonSerializer.Serialize(AppLog));
            }
            else if (typeLog.Equals("Error"))
            {
                AppLog.Level = 40;
                Log.Error(JsonSerializer.Serialize(AppLog));
            }
            else if (typeLog.Equals("Debug"))
            {
                AppLog.Level = 10;
                Log.Debug(JsonSerializer.Serialize(AppLog));
            }
        }

        private Aspirante buscarAspirante(string NoExpediente)
        {
            Aspirante resultado = null;
            SqlDataAdapter daAspirante = new SqlDataAdapter($"select * from Aspirante a where a.NoExpediente = '{NoExpediente}'", connection);
            DataSet dsAspirante = new DataSet();
            daAspirante.Fill(dsAspirante, "Aspirante");
            if (dsAspirante.Tables["Aspirante"].Rows.Count > 0)
            {
                resultado = new Aspirante()
                {
                    NoExpediente = dsAspirante.Tables["Aspirante"].Rows[0][0].ToString(),
                    Apellidos = dsAspirante.Tables["Aspirante"].Rows[0][1].ToString(),
                    Nombres = dsAspirante.Tables["Aspirante"].Rows[0][2].ToString(),
                    Direccion = dsAspirante.Tables["Aspirante"].Rows[0][3].ToString(),
                    Telefono = dsAspirante.Tables["Aspirante"].Rows[0][4].ToString(),
                    Email = dsAspirante.Tables["Aspirante"].Rows[0][5].ToString(),
                    Estatus = dsAspirante.Tables["Aspirante"].Rows[0][6].ToString(),
                    CarreraId = dsAspirante.Tables["Aspirante"].Rows[0][7].ToString(),
                    JornadaId = dsAspirante.Tables["Aspirante"].Rows[0][9].ToString(),
                };
            }

            /*for (int i = 0; i < aspirantes.Count; i++)
            {
                if (aspirantes[i].NoExpediente == NoExpediente)
                {
                    resultado = aspirantes[i];
                    break;
                }
            }*/
            return resultado;
        }

        public string Test(string s)
        {
            Console.WriteLine("Test method executed");
            return s;
        }
    }
}