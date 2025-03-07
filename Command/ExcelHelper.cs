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
    public class ExcelHelper<T> where T : class, new()
    {
        private string _filePath;



        // 动态生成Excel文件
        public void CreateExcelWithHeaders()
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");

            // 获取 T 类型的所有属性
            var properties = typeof(T).GetProperties();
            var headerRow = sheet.CreateRow(0); // 创建标题行

            // 根据属性名生成标题
            for (int i = 0; i < properties.Length; i++)
            {
                headerRow.CreateCell(i).SetCellValue(properties[i].Name);
            }

            using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(stream);
            }
        }

        // 读取Excel数据到 List<T>
        public List<T> ReadItemsFromExcel(string filePath)
        {
            _filePath=filePath;
            var items = new List<T>();

            if (!File.Exists(_filePath))
            {
                return items;
                //throw new FileNotFoundException("Excel文件未找到");
            }

            using (var file = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheetAt(0);
                int rowCount = sheet.LastRowNum;
                var properties = typeof(T).GetProperties(); // 获取属性列表

                for (int row = 1; row <= rowCount; row++) // 跳过标题行
                {
                    IRow currentRow = sheet.GetRow(row);
                    if (currentRow == null) continue; // 跳过空行

                    var item = Activator.CreateInstance<T>(); // 创建对象实例

                    for (int col = 0; col < properties.Length; col++)
                    {
                        var property = properties[col];
                        var cell = currentRow.GetCell(col);

                        if (cell != null)
                        {
                            // 设置属性的值
                            if (property.PropertyType == typeof(int))
                            {
                                if (cell.CellType == CellType.Numeric)
                                {
                                    property.SetValue(item, Convert.ToInt32(cell.NumericCellValue));
                                }
                            }
                            else if (property.PropertyType == typeof(string))
                            {
                                if (cell.CellType == CellType.String)
                                {
                                    property.SetValue(item, cell.StringCellValue);
                                }
                                else if (cell.CellType == CellType.Numeric) // 如果是数字类型，则转换为字符串
                                {
                                    property.SetValue(item, cell.NumericCellValue.ToString());
                                }
                            }
                            else if (property.PropertyType == typeof(DateTime)) // 处理 DateTime 类型
                            {
                                if (cell.CellType == CellType.Numeric) // 日期在 Excel 中通常以数值形式存储
                                {
                                    property.SetValue(item, cell.DateCellValue);
                                }
                                else if (cell.CellType == CellType.String) // 如果是字符串格式的日期
                                {
                                    if (DateTime.TryParse(cell.StringCellValue, out DateTime dateValue))
                                    {
                                        property.SetValue(item, dateValue);
                                    }
                                }
                            }
                        }
                    }

                    items.Add(item);
                }
            }

            return items;
        }

        // 添加新数据到Excel
        public void AddItemToExcel(T item)
        {
            using (var file = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheetAt(0);
                int rowCount = sheet.LastRowNum + 1;

                var properties = typeof(T).GetProperties();
                IRow newRow = sheet.CreateRow(rowCount);

                for (int i = 0; i < properties.Length; i++)
                {
                    var value = properties[i].GetValue(item);
                    if (value != null)
                    {
                        if (properties[i].PropertyType == typeof(int))
                        {
                            newRow.CreateCell(i).SetCellValue((int)value);
                        }
                        else if (properties[i].PropertyType == typeof(string))
                        {
                            newRow.CreateCell(i).SetCellValue(value.ToString());
                        }
                    }
                }

                using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(stream);
                }
            }
        }

        // 编辑Excel中的数据
        public void UpdateItemInExcel(ItemModel item)
        {
            using (var file = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheetAt(0);
                int rowCount = sheet.LastRowNum;

                for (int row = 1; row <= rowCount; row++)
                {
                    IRow currentRow = sheet.GetRow(row);
                    if (Convert.ToInt32(currentRow.GetCell(0).NumericCellValue) == item.Index)
                    {
                        currentRow.GetCell(1).SetCellValue(item.Name);
                        currentRow.GetCell(2).SetCellValue(item.Type);
                        currentRow.GetCell(3).SetCellValue(item.Unit);
                        break;
                    }
                }

                using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(stream);
                }
            }
        }

        // 删除Excel中的数据
        public void DeleteItemFromExcel(int index)
        {
            using (var file = new FileStream(_filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                IWorkbook workbook = new XSSFWorkbook(file);
                ISheet sheet = workbook.GetSheetAt(0);
                int rowCount = sheet.LastRowNum;

                for (int row = 1; row <= rowCount; row++)
                {
                    IRow currentRow = sheet.GetRow(row);
                    if (currentRow == null) continue; // 检查当前行是否为空

                    // 获取单元格并检查其类型
                    ICell cell = currentRow.GetCell(0);
                    if (cell == null) continue; // 如果单元格为空，则跳过

                    // 确保单元格是数字类型
                    if (cell.CellType == CellType.Numeric)
                    {
                        // 将单元格的数值转换为整数
                        int cellValue = Convert.ToInt32(cell.NumericCellValue);
                        if (cellValue == index)
                        {
                            sheet.RemoveRow(currentRow); // 删除当前行

                            // 确保将后面的行上移
                            for (int r = row + 1; r <= rowCount; r++)
                            {
                                IRow rowToShift = sheet.GetRow(r);
                                if (rowToShift != null)
                                {
                                    // 将当前行数据移动到上一行
                                    sheet.ShiftRows(r, rowCount, -1);
                                }
                            }
                            break; // 找到并删除后跳出循环
                        }
                    }
                    else if (cell.CellType == CellType.String)
                    {
                        // 如果是字符串类型，可以根据需要转换为整型，例如：
                        int cellValue;
                        if (int.TryParse(cell.StringCellValue, out cellValue) && cellValue == index)
                        {
                            sheet.RemoveRow(currentRow); // 删除当前行

                            // 确保将后面的行上移
                            for (int r = row + 1; r <= rowCount; r++)
                            {
                                IRow rowToShift = sheet.GetRow(r);
                                if (rowToShift != null)
                                {
                                    // 将当前行数据移动到上一行
                                    sheet.ShiftRows(r, rowCount, -1);
                                }
                            }
                            break; // 找到并删除后跳出循环
                        }
                    }
                }

                using (var stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(stream);
                }
            }
        }

    }
}
