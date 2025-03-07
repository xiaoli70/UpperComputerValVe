using EquipmentSignalData.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataService.Entity;

namespace EquipmentSignalData.Command
{
    public class ExcelLogger
    {
        private readonly string _filePath;
        private IWorkbook _workbook;
        private ISheet _worksheet;

        public ExcelLogger()
        {
            // 获取项目根目录
            string rootPath = AppDomain.CurrentDomain.BaseDirectory;

            // 构建日志文件的完整路径
            string directoryPath = Path.Combine(rootPath, "database", "Log");
            _filePath = Path.Combine(directoryPath, $"operationLogs{DateTime.Now:yyyyMMdd}.xlsx");

            // 如果日志文件夹不存在，则创建文件夹
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 初始化日志文件，如果文件不存在则创建，并添加表头
            if (!File.Exists(_filePath))
            {
                using (var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    _workbook = new XSSFWorkbook();
                    _worksheet = _workbook.CreateSheet("Logs");
                    var headerRow = _worksheet.CreateRow(0);
                    headerRow.CreateCell(0).SetCellValue("ValveName");
                    headerRow.CreateCell(1).SetCellValue("Timestamp");
                    headerRow.CreateCell(2).SetCellValue("OperationName");
                    headerRow.CreateCell(3).SetCellValue("Detail");
                    //headerRow.CreateCell(3).SetCellValue("Detail");
                    _workbook.Write(fileStream);
                }
            }
            else
            {
                // 如果文件已存在，则打开工作簿
                using (var fileStream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
                {
                    _workbook = new XSSFWorkbook(fileStream);
                    _worksheet = _workbook.GetSheet("Logs");
                }
            }
        }

        // 记录日志
        // 异步记录日志
        public async Task LogOperationAsync(OperationLog operationLog)
        {
            // 创建新的行并写入日志
            int row = _worksheet.LastRowNum + 1; // 获取最后一行的下一个位置
            var newRow = _worksheet.CreateRow(row);

            newRow.CreateCell(0).SetCellValue(operationLog.ValveName);
            newRow.CreateCell(1).SetCellValue(DateTime.Now.ToString());
            newRow.CreateCell(2).SetCellValue(operationLog.OperationName);
            newRow.CreateCell(3).SetCellValue(operationLog.Detail);

            // 保存更改到文件，使用异步写入
            using (var fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await Task.Run(() => _workbook.Write(fileStream));
            }
        }
    }
}
