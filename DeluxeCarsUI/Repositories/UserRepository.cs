using DeluxeCarsUI.Model;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BCrypt.Net;



namespace DeluxeCarsUI.Repositories
{
    public class UserRepository : RepositoryBase, IUserRepository
    {

        public void Add(UserModel userModel)
        {
            // Validaciones
            if (userModel == null)
                throw new ArgumentNullException(nameof(userModel));

            if (string.IsNullOrWhiteSpace(userModel.Username))
                throw new ArgumentException("Username es requerido", nameof(userModel.Username));

            if (string.IsNullOrWhiteSpace(userModel.Password))
                throw new ArgumentException("Password es requerido", nameof(userModel.Password));

            try
            {
                using (var connection = GetConnection())
                using (var cmd = new SqlCommand())
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandText = @"INSERT INTO [User] (username, [password], name, lastname, email) 
                                       VALUES (@username, @password, @name, @lastname, @email)";

                    cmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = userModel.Username;
                    // Hashear la contraseña antes de guardarla
                    cmd.Parameters.Add("@password", SqlDbType.NVarChar).Value = BCrypt.Net.BCrypt.HashPassword(userModel.Password);
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = userModel.Name ?? string.Empty;
                    cmd.Parameters.Add("@lastname", SqlDbType.NVarChar).Value = userModel.LastName ?? string.Empty;
                    cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = userModel.Email ?? string.Empty;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                // Log del error (usar tu sistema de logging)
                throw new InvalidOperationException($"Error al agregar usuario: {ex.Message}", ex);
            }
        }

        public bool AuthenticateUser(NetworkCredential credential)
        {
            if (credential == null || string.IsNullOrWhiteSpace(credential.UserName))
                return false;

            try
            {
                using (var connection = GetConnection())
                using (var cmd = new SqlCommand())
                {
                    connection.Open();
                    cmd.Connection = connection;

                    // 1) Obtiene el valor actual de la columna [password] para ese usuario
                    cmd.CommandText = "SELECT [password] FROM [User] WHERE username = @username";
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = credential.UserName;

                    var storedPassword = cmd.ExecuteScalar()?.ToString();
                    if (string.IsNullOrEmpty(storedPassword))
                        return false; // Usuario inexistente

                    bool esHashBCrypt = false;

                    // 2) Detecta si "storedPassword" cumple con el formato típico de BCrypt
                    //    Un hash de BCrypt comienza por "$2a$", "$2b$" o "$2y$" y tiene longitud 60.
                    if (storedPassword.StartsWith("$2") && storedPassword.Length == 60)
                        esHashBCrypt = true;

                    // 3) Si NO es un hash BCrypt, asumimos que es texto plano
                    if (!esHashBCrypt)
                    {
                        // Compara el texto ingresado por el usuario con el texto plano en la DB
                        if (credential.Password == storedPassword)
                        {
                            // Coincide: ahora debemos rehashear y actualizar el registro
                            string nuevoHash = HashPassword(credential.Password);

                            using (var updateCmd = new SqlCommand())
                            {
                                updateCmd.Connection = connection;
                                updateCmd.CommandText = "UPDATE [User] SET [password] = @newHash WHERE username = @username";
                                updateCmd.Parameters.Add("@newHash", SqlDbType.NVarChar).Value = nuevoHash;
                                updateCmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = credential.UserName;
                                updateCmd.ExecuteNonQuery();
                            }

                            // Acceso concedido, y a partir de ahora ya hay un hash BCrypt válido en la DB
                            return true;
                        }
                        else
                        {
                            // Si el texto plano no coincide, deniega acceso
                            return false;
                        }
                    }
                    else
                    {
                        // 4) Si ya es un hash BCrypt, usamos Verify normalmente
                        bool esValido = BCrypt.Net.BCrypt.Verify(credential.Password, storedPassword);
                        return esValido;
                    }
                }
            }
            catch (SqlException)
            {
                // Si ocurre cualquier excepción, por seguridad denegamos acceso
                return false;
            }
        }

        public void Edit(UserModel userModel)
        {
            if (userModel == null)
                throw new ArgumentNullException(nameof(userModel));

            if (string.IsNullOrWhiteSpace(userModel.Id))
                throw new ArgumentException("ID es requerido", nameof(userModel.Id));

            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    string passwordToUse;

                    if (!string.IsNullOrWhiteSpace(userModel.Password))
                    {
                        // Se proporcionó nueva contraseña
                        passwordToUse = HashPassword(userModel.Password);
                    }
                    else
                    {
                        // Obtener contraseña actual de la base de datos
                        using (var selectCmd = new SqlCommand("SELECT [password] FROM [User] WHERE id = @id", connection))
                        {
                            selectCmd.Parameters.Add("@id", SqlDbType.Int).Value = int.Parse(userModel.Id);

                            var result = selectCmd.ExecuteScalar();
                            if (result == null || string.IsNullOrWhiteSpace(result.ToString()))
                                throw new InvalidOperationException("No se pudo obtener la contraseña actual.");

                            passwordToUse = result.ToString();
                        }
                    }

                    // Ahora sí ejecutamos el UPDATE
                    using (var updateCmd = new SqlCommand(@"UPDATE [User] 
                                                    SET username=@username, [password]=@password, 
                                                        name=@name, lastname=@lastname, email=@email 
                                                    WHERE id=@id", connection))
                    {
                        updateCmd.Parameters.Add("@id", SqlDbType.Int).Value = int.Parse(userModel.Id);
                        updateCmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = userModel.Username;
                        updateCmd.Parameters.Add("@password", SqlDbType.NVarChar).Value = passwordToUse;
                        updateCmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = userModel.Name ?? string.Empty;
                        updateCmd.Parameters.Add("@lastname", SqlDbType.NVarChar).Value = userModel.LastName ?? string.Empty;
                        updateCmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = userModel.Email ?? string.Empty;

                        updateCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Error al actualizar usuario: {ex.Message}", ex);
            }
        }

        public IEnumerable<UserModel> GetByAll()
        {
            using (var connection = GetConnection())
            using (var cmd = new SqlCommand())
            {
                connection.Open();
                cmd.Connection = connection;
                cmd.CommandText = "SELECT id, username, name, lastname, email FROM [User]"; 

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return new UserModel()
                        {
                            Id = reader["id"].ToString(),           
                            Username = reader["username"].ToString(),
                            Password = string.Empty, // Nunca retornar password
                            Name = reader["name"].ToString(),
                            LastName = reader["lastname"].ToString(),
                            Email = reader["email"].ToString(),
                        };
                    }
                }
            }
        }

        public UserModel GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID debe ser mayor a 0", nameof(id));

            try
            {
                using (var connection = GetConnection())
                using (var cmd = new SqlCommand())
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT id, username, name, lastname, email FROM [User] WHERE id=@id";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel()
                            {
                                Id = reader["id"].ToString(),
                                Username = reader["username"].ToString(),
                                Password = string.Empty,
                                Name = reader["name"].ToString(),
                                LastName = reader["lastname"].ToString(),
                                Email = reader["email"].ToString(),
                            };
                        }
                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Error al obtener usuario por ID: {ex.Message}", ex);
            }
        }

        public UserModel GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username no puede estar vacío", nameof(username));

            try
            {
                using (var connection = GetConnection())
                using (var cmd = new SqlCommand())
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT id, username, name, lastname, email FROM [User] WHERE username=@username";
                    cmd.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel()
                            {
                                Id = reader["id"].ToString(),
                                Username = reader["username"].ToString(),
                                Password = string.Empty,
                                Name = reader["name"].ToString(),
                                LastName = reader["lastname"].ToString(),
                                Email = reader["email"].ToString(),
                            };
                        }
                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Error al obtener usuario por username: {ex.Message}", ex);
            }
        }

        public UserModel GetByUserEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email no puede estar vacío", nameof(email));

            try
            {
                using (var connection = GetConnection())
                using (var cmd = new SqlCommand())
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT id, username, name, lastname, email FROM [User] WHERE email=@email";
                    cmd.Parameters.Add("@email", SqlDbType.NVarChar).Value = email;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserModel()
                            {
                                Id = reader["id"].ToString(),
                                Username = reader["username"].ToString(),
                                Password = string.Empty,
                                Name = reader["name"].ToString(),
                                LastName = reader["lastname"].ToString(),
                                Email = reader["email"].ToString(),
                            };
                        }
                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Error al obtener usuario por email: {ex.Message}", ex);
            }
        }

        public void Remove(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID debe ser mayor a 0", nameof(id));

            try
            {
                using (var connection = GetConnection())
                using (var cmd = new SqlCommand())
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandText = "DELETE FROM [User] WHERE id=@id";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Error al eliminar usuario: {ex.Message}", ex);
            }
        }

        public int GetUserIdByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email no puede estar vacío", nameof(email));
            try
            {
                using (var connection = GetConnection())
                using (var cmd = new SqlCommand())
                {
                    connection.Open();
                    cmd.Connection = connection;
                    cmd.CommandText = "SELECT id FROM [User] WHERE email=@correo";
                    cmd.Parameters.Add("@correo", SqlDbType.NVarChar).Value = email;
                    var result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : -1; // Retorna -1 si no se encuentra el usuario
                }
            }
            catch (SqlException ex)
            {
                throw new InvalidOperationException($"Error al obtener ID de usuario por email: {ex.Message}", ex);
            }
        }

        public bool RecoverPassword(string correo)
        {
            // Verificar si el usuario existe y obtener su id
            int? userId = GetUserIdByEmail(correo);
            if (userId == null) return false;

            // Generar Token
            string token = GenerarToken();

            // Calcular fecha de expiraciion (15 minutos a partir de ahora)
            DateTime ahora = DateTime.Now;
            DateTime expiracion = ahora.AddMinutes(15);

            // Guardar en la tabla PasswordResets
            GuardarTokenEnBaseDeDatos(correo, token, expiracion);

            // Enviar correo al usuario con el token
            /*var emailService = new EmailService();
            emailService.EnviarCorreoRecuperacion(correo, token); */

            return true; // Indica que se inició el proceso de recuperación correctamente
        }

        private string GenerarToken()
        {
            // Genera un token de 6 caracteres alfanuméricos
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(); 
        }

        private void GuardarTokenEnBaseDeDatos(string correo, string token, DateTime expiration)
        {
            using (var connection = GetConnection())
            using (var cmd = new SqlCommand(@"
                INSERT INTO PasswordResets (Correo, Token, FechaCreacion, FechaExpiracion, Usado)
                VALUES (@correo, @token, GETDATE(), @fechaExpiracion, 0)", connection))
            {
                cmd.Parameters.AddWithValue("@correo", correo);
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@fechaExpiracion", expiration);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public bool ValidateTokenAndResetPassword(string correo, string token, string nuevaPassword)
        {
            // Recuperar registro en PasswordResets
            int? resetId = null;
            using (var connection = GetConnection())
            using (var cmd = new SqlCommand(@"
                SELECT Id
                FROM PasswordResets
                WHERE Correo = @Correo
                AND Token = @Token 
                AND Usado = 0 
                AND FechaExpiracion > GETDATE()
                ", connection))
            {
                cmd.Parameters.AddWithValue("@Correo", correo);
                cmd.Parameters.AddWithValue("@Token", token);
                connection.Open();
                var result = cmd.ExecuteScalar();
                //resetId = result == null ? (int)null : Convert.ToInt32(result);
            }

            if (resetId == null)
                return false; // Token inválido o expirado

            // Actualizar contraseña del usuario
            ActualizarPasswordUsuario(correo, nuevaPassword);

            // 3. Marcar token como usado en PasswordResets
            using (var connection = GetConnection())
            using (var cmd = new SqlCommand(@"
                UPDATE PasswordResets 
                SET Usado = 1 
                WHERE Id = @Id
            ", connection))
            {
                cmd.Parameters.AddWithValue("@Id", resetId.Value);
                connection.Open();
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        private void ActualizarPasswordUsuario(string correo, string nuevaPassword)
        {
            // Aquí conviene almacenar la contraseña encriptada (por ejemplo, SHA256 + salt)
            string passwordHash = HashPassword(nuevaPassword);

            using (var connection = GetConnection())
            using (var cmd = new SqlCommand("UPDATE Usuarios SET PasswordHash = @Hash WHERE Email = @Email", connection))
            {
                cmd.Parameters.AddWithValue("@Hash", passwordHash);
                cmd.Parameters.AddWithValue("@Email", correo);
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private string HashPassword(string password)
        {
            // Ejemplo sencillo con SHA256 (mejor usar salt y pbkdf2 en producción)
            using (var sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

    }
}
