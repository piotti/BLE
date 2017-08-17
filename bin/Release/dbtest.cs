using Npgsql;
using System;

class DBTest {

    public static void Main() {
        var connString = "Host=localhost;Username=postgres;Password=JensenRobot321;Database=postgres";

        using (var conn = new NpgsqlConnection(connString))
        {
            conn.Open();
            /*
            // Insert some data
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = Console.ReadLine();
                cmd.ExecuteNonQuery();
            }


            // Retrieve all rows
            Console.WriteLine("pres_1:");
            using (var cmd = new NpgsqlCommand("SELECT pres_1 FROM real_time", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    Console.WriteLine(reader.GetString(0));

            Console.WriteLine("pres_2:");
            using (var cmd = new NpgsqlCommand("SELECT pres_2 FROM real_time", conn))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    Console.WriteLine(reader.GetString(0));

            */

            conn.Close();
        }

    }  
}