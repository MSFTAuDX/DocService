﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DocServiceCore.Models;
using DocServiceCore.Services;
using System.Web.Http;
using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocServiceCore.Helpers;
using System.Net;
using DocumentFormat.OpenXml;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace DocServiceCore.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class DocumentController : Controller
    {
        // GET: api/values
        [Microsoft.AspNetCore.Mvc.HttpGet]
        public IEnumerable<Doc> Get()
        {
            //TODO Get all of the Docs in the list
            return DataService.GetDocumentHeaders();
        }

        // GET api/values/5
        [Microsoft.AspNetCore.Mvc.HttpGet("{id}")]
        public async Task<ActionResult> Get(Guid id)
        {
            {
                try
                {
                    var fullDoc = DataService.getFullDoc(id);

                    IHttpActionResult result;
                    MemoryStream mem = new MemoryStream();


                    // Create Document
                    using (WordprocessingDocument wordDocument =
                        WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
                    {
                        // Add a main document part. 
                        MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                        DocHelper.AddStyles(mainPart);

                        // Create the document structure and add some text.
                        mainPart.Document = new Document();
                        Body body = mainPart.Document.AppendChild(new Body());

                        // Title and Sub-title
                        Paragraph titlePara = body.AppendChild(new Paragraph());
                        DocHelper.ApplyStyleToParagraph(wordDocument, "unknown", "Title", titlePara);
                        Run run = titlePara.AppendChild(new Run());
                        run.AppendChild(new Text(fullDoc.Header.Title));

                        Paragraph subTitlePara = body.AppendChild(new Paragraph());
                        DocHelper.ApplyStyleToParagraph(wordDocument, "unknown", "Subtitle", subTitlePara);
                        subTitlePara.AppendChild(new Run(new Text($"Created {fullDoc.Header.Created} (UTC)")));

                        // Paragraph for each para in the list
                        foreach (var para in fullDoc.Paragraphs)
                        {
                            var paragraph = body.AppendChild(new Paragraph(new Run(new Text(
                                $"[{para.TimeStamp} (UTC)] - {para.Text}"))));
                            if (!string.IsNullOrWhiteSpace(para.Style))
                                DocHelper.ApplyStyleToParagraph(wordDocument, "unknown", para.Style, paragraph);
                        }


                        mainPart.Document.Save();
                    }

                    mem.Seek(0, SeekOrigin.Begin);

                    return File(mem, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fullDoc.Header.FileName);


                }
                catch (KeyNotFoundException e)
                {
                    Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return null;
                }

            }
        }

        // POST api/values
        [System.Web.Http.HttpPost]
        public async Task<ActionResult> Post([System.Web.Http.FromBody]Doc value)
        {
            value.Id = Guid.NewGuid();
            value.Created = DateTimeOffset.UtcNow;

            if (string.IsNullOrWhiteSpace(value.Title))
                value.Title = "Transcript";

            if (string.IsNullOrWhiteSpace(value.FileName))
                value.FileName = $"{value.Id}.docx";


            Doc newDoc = await DataService.AddDocument(value);

            return Json(newDoc);
        }

        // PUT api/document/{id}
        [Microsoft.AspNetCore.Mvc.HttpPut("{id}")]
        public async Task<ActionResult> Put(Guid id, [Microsoft.AspNetCore.Mvc.FromBody]Para value)
        {
            try
            {
                value.DocId = id;
                return Json(await DataService.AddParagraph(value));
            }
            catch (KeyNotFoundException)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return null;
            }
        }

        // DELETE api/values/5
        [Microsoft.AspNetCore.Mvc.HttpDelete("{id}")]
        public async Task<IHttpActionResult> Delete(Guid id)
        {
            try
            {
                await DataService.DeleteDoc(id);
                return null;
            }
            catch (KeyNotFoundException)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return null;
            }
        }
    }
}