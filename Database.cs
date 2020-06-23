using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace kanazawa.Function
{
    public class Database
    {
        public static void checkMasterData(List<QiitaInformationModel> models, ILogger log, SqlConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var selectCommand = new SqlCommand() { Connection = connection, Transaction = transaction })
                    {
                        // SQLの準備
                        selectCommand.CommandText = @"SELECT id FROM qiita_items";

                        // SQLの実行
                        var table = new DataTable();
                        var adapter = new SqlDataAdapter(selectCommand);
                        adapter.Fill(table);

                        // 存在フラグ
                        bool flg = false;

                        foreach (var model in models)
                        {
                            flg = false;
                            for (int i = 0; i < table.Rows.Count; i++)
                            {
                                if (table.Rows[i]["id"].ToString().Equals(model.Id))
                                {
                                    flg = true;
                                    break;
                                }
                            }

                            if (flg == false)
                            {
                                using (var insertCommand = new SqlCommand() { Connection = connection, Transaction = transaction })
                                {
                                    // SQLの準備
                                    insertCommand.CommandText = @"INSERT INTO qiita_items VALUES (@ID, @TITLE, @CREATED_AT)";
                                    insertCommand.Parameters.Add(new SqlParameter("@ID", model.Id));
                                    insertCommand.Parameters.Add(new SqlParameter("@TITLE", model.Title));
                                    insertCommand.Parameters.Add(new SqlParameter("@CREATED_AT", model.CreatedAt));

                                    // SQLの実行
                                    insertCommand.ExecuteNonQuery();

                                    log.LogInformation($"succeeded to insert master data: {model.Title}");
                                }
                            }
                        }
                    }

                    // コミット
                    transaction.Commit();
                    log.LogInformation("Committed");
                }
                catch
                {
                    // ロールバック
                    transaction.Rollback();
                    log.LogInformation("Rollbacked");
                    throw;
                }
            }
        }

        public static void saveData(List<QiitaInformationModel> models, ILogger log, SqlConnection connection)
        {
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var model in models)
                    {
                        using (var command = new SqlCommand() { Connection = connection, Transaction = transaction })
                        {
                            // SQLの準備
                            command.CommandText = @"INSERT INTO page_views_count VALUES (@ID, @COUNTED_AT, @PAGE_VIEWS_COUNT)";
                            command.Parameters.Add(new SqlParameter("@ID", model.Id));
                            command.Parameters.Add(new SqlParameter("@COUNTED_AT", DateTime.Now.ToString("yyyy/MM/dd HH")));
                            command.Parameters.Add(new SqlParameter("@PAGE_VIEWS_COUNT", model.PageViewsCount));

                            // SQLの実行
                            command.ExecuteNonQuery();

                            log.LogInformation($"succeeded to insert data: {model.Title}");
                        }
                    }

                    // コミット
                    transaction.Commit();
                    log.LogInformation("Committed");
                }
                catch
                {
                    // ロールバック
                    transaction.Rollback();
                    log.LogInformation("Rollbacked");
                    throw;
                }
            }
        }
    }
}