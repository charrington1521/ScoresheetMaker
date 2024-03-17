using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ScoresheetMaker.Models;

namespace ScoresheerMaker.Controllers 
{ 
    public class PdfController : Controller 
    { 
        private readonly IWebHostEnvironment _environment;

        private IServiceScopeFactory _serviceScopeFactory;

        public PdfController(IWebHostEnvironment environment, IServiceScopeFactory serviceScopeFactory)
        {
            _environment = environment;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public ViewResult Index(string pdfName)
        {
            return View("Pdf");
        }

        public FileResult GetPdf()
        {
            PdfModel model = newModel("outputScoreSheet.pdf");
            FileStream fs = new FileStream(model.PdfPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return File(fs, "application/pdf");
        }

        private PdfModel newModel(string pdfName)
        {
            PdfModel model = new PdfModel();
            
            model.PdfPath = Path.Combine(_environment.WebRootPath, "documents");
            model.PdfPath = Path.Combine(model.PdfPath, pdfName);

            return model;
        }
    } 
}