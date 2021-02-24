using System;
using System.Collections.Generic;
using System.Linq;
using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Files;

namespace FytSugar.Builder
{
    public class BuilderService:IBuilderService
    {
        /// <summary>
        /// 连接数据库，并返回当前连接下所有数据库名字
        /// </summary>
        /// <returns></returns>
        public ApiResult<List<string>> InitConnection(BuilderConnection param)
        {
            var result = JResult<List<string>>.Success();
            try
            {
                var db = new SugarInstance().GetInstance(param);
                result.Data=db.DbMaintenance.GetTableInfoList().Select(m=>m.Name).ToList();
                return result;
            }
            catch (Exception ex)
            {
                return JResult<List<string>>.Error(ex.Message);
            }
        }

        /// <summary>
        /// 生成代码
        /// </summary>
        /// <returns></returns>
        public ApiResult<string> CreateCode(BuilderModel createModel)
        {
            var result = JResult<string>.Success();
            try
            {
                var _tempPath = DateTime.Now.ToString("yyyyMMdd");
                var path = "/wwwroot/generate/" + _tempPath;
                FileHelper.CreateFiles("/wwwroot/generate/zip/");
                var db = new SugarInstance().GetInstance(createModel.connection);
                //读取模板
                var strTemp = FileHelper.ReadFile("/Template/Model.html");
                //仓储接口
                var irepositoryTemp = FileHelper.ReadFile("/Template/IRepository.html");
                //仓储实现
                var repositoryTemp = FileHelper.ReadFile("/Template/Repository.html");
                //服务接口
                var iserviceTemp = FileHelper.ReadFile("/Template/IService.html");
                //服务实现
                var serviceTemp = FileHelper.ReadFile("/Template/Service.html");
                //Api控制器
                var apiTemp = FileHelper.ReadFile("/Template/ApiController.html");
                var tableList = new List<string>();
                if (createModel.Types==1)
                {
                    tableList= db.DbMaintenance.GetTableInfoList().Select(m => m.Name).ToList();
                }
                else
                {
                    tableList = createModel.TableNames.ToList();
                }
                foreach (var row in tableList)
                {
                    var column = db.DbMaintenance.GetColumnInfosByTableName(row);
                    //构建属性
                    string attrStr = "", tableColumn="";
                    foreach (var item in column)
                    {
                        attrStr += "        /// <summary>\r\n";
                        attrStr += "        /// " + item.ColumnDescription + "\r\n";
                        attrStr += "        /// <summary>\r\n";
                        if (item.IsPrimarykey)
                        {
                            attrStr += "        [SugarColumn(IsPrimaryKey = true)]\r\n";
                        }
                        attrStr += "        public " + item.DataType.ConvertModelType() + " " + item.DbColumnName + " { get; set; }" + item.DataType.ModelDefaultValue(item.DefaultValue) + "\r\n\r\n";
                    }
                    var modelName = row.TableName();
                    tableColumn = strTemp
                       .Replace("{NameSpace}", "FytSoa.Domain.Models."+createModel.Namespace)
                       .Replace("{DataTable}", row)
                       .Replace("{TableName}", modelName)
                       .Replace("{AttributeList}", attrStr);
                    //写入文件
                    FileHelper.WriteFile(path + "/Model/", modelName + ".cs", tableColumn);

                    //仓储接口
                    string irepositoryString = irepositoryTemp.Replace("{NameSpace}",createModel.Namespace)
                        .Replace("{TableName}",modelName);
                    FileHelper.WriteFile(path + "/IRepository/", "I"+modelName + "Repository.cs", irepositoryString);

                    //仓储实现
                    string repositoryString = repositoryTemp.Replace("{NameSpace}", createModel.Namespace)
                        .Replace("{TableName}", modelName);
                    FileHelper.WriteFile(path + "/Repository/", modelName + "Repository.cs", repositoryString);

                    //服务接口
                    string iserviceString = iserviceTemp.Replace("{NameSpace}", createModel.Namespace)
                        .Replace("{TableName}", modelName);
                    FileHelper.WriteFile(path + "/IService/", "I" + modelName + "Service.cs", iserviceString);

                    //服务实现
                    string serviceString = serviceTemp.Replace("{NameSpace}", createModel.Namespace)
                        .Replace("{TableName}", modelName);
                    FileHelper.WriteFile(path + "/Service/", modelName + "Service.cs", serviceString);

                    //Api控制器
                    string apiString = apiTemp.Replace("{NameSpace}", createModel.Namespace)
                        .Replace("{TableName}", modelName)
                        .Replace("{ApiName}", createModel.ControllerName);
                    FileHelper.WriteFile(path + "/Controller/", createModel.ControllerName + "Controller.cs", apiString);
                }
                var nowpath = FileHelper.MapPath("/wwwroot/generate/zip/"+ _tempPath + ".zip");
                ZipHelper.CreateZip(FileHelper.MapPath(path), nowpath);
                result.Data = _tempPath;
                return result;
            }
            catch (Exception ex)
            {
                return JResult<string>.Error(ex.Message);
            }
        }
    }
}
