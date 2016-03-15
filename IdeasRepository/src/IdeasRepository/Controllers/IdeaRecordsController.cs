using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using IdeasRepository.Models;
using Microsoft.AspNet.Authorization;

namespace IdeasRepository.Controllers
{
    public class IdeaRecordsController : Controller
    {
        private ApplicationDbContext _context;

        public IdeaRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: IdeaRecords
        [Authorize]
        public IActionResult Index(int id = 0, bool showAllRecords = false)
        {
            ViewData["ShowAllRecords"] = showAllRecords;
            IQueryable<IdeaRecord> allrecords = _context.IdeaRecord.Include(i => i.Author);
            IQueryable<IdeaRecord> applicationDbContext =
                allrecords.Where(
                    l =>
                        l.Status != IdeaRecordStatusEnum.ArchiveByAdmin &&
                        l.Status != IdeaRecordStatusEnum.ArchiveByUser && 
                        (showAllRecords || l.Status == IdeaRecordStatusEnum.Created));
            if (User.IsInRole("Administrator"))
            {
                ViewData["RemovementRequestsCount"] =
                    allrecords.Count(l => l.Status == IdeaRecordStatusEnum.RemovedByUser);
            }
            else
            {
                applicationDbContext =
                    allrecords.Where(l => l.AuthorId == User.GetUserId() && l.Status == IdeaRecordStatusEnum.Created);
                ViewData["RemovementRequestsCount"] =
                    allrecords.Count(
                        l => l.AuthorId == User.GetUserId() && l.Status == IdeaRecordStatusEnum.RemovedByAdmin);
            }
            ViewData["Pages"] = (int)Math.Ceiling((decimal)applicationDbContext.Count()/20);
            id = id > (int) ViewData["Pages"] - 1 ? (int) ViewData["Pages"] - 1 : id;
            ViewData["Page"] = id;
            return View(applicationDbContext.OrderByDescending(l => l.Date).Skip(id*20).Take(20).ToList());
        }

        // GET: IdeaRecords/Details/5
        [Authorize]
        public IActionResult Details(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }
            IdeaRecord ideaRecord =
                _context.IdeaRecord.Include(c => c.Author).Include(c => c.Editor).Single(m => m.ID == id);
            if (ideaRecord == null)
            {
                return HttpNotFound();
            }
            if (User.GetUserId() != ideaRecord.AuthorId && !User.IsInRole("Administrator"))
            {
                return RedirectToAction("Index");
            }
            return View(ideaRecord);
        }

        // GET: IdeaRecords/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        // POST: IdeaRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Create(IdeaRecord ideaRecord)
        {
            if (ModelState.IsValid)
            {
                var now = DateTime.Now;
                _context.IdeaRecord.Add(new IdeaRecord
                {
                    Text = ideaRecord.Text,
                    Prewiew = IdeaRecord.GetPrewiew(ideaRecord.Text),
                    Date = now,
                    Title = ideaRecord.Title,
                    AuthorId = User.GetUserId(),
                    Status = IdeaRecordStatusEnum.Created,
                    StatusDate = now,
                    ID = Guid.NewGuid().ToString()
                });
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ideaRecord);
        }

        // GET: IdeaRecords/Edit/5
        [Authorize]
        public IActionResult Edit(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            IdeaRecord ideaRecord =
                _context.IdeaRecord.Include(l => l.Author).Include(l => l.Editor).Single(m => m.ID == id);
            if (ideaRecord == null)
            {
                return HttpNotFound();
            }
            if (ideaRecord.Status != IdeaRecordStatusEnum.Created ||
                (User.GetUserId() != ideaRecord.AuthorId && !User.IsInRole("Administrator")))
            {
                return RedirectToAction("Index");
            }
            return View(ideaRecord);
        }

        // POST: IdeaRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Edit(IdeaRecord ideaRecord)
        {
            if (ideaRecord.Status != IdeaRecordStatusEnum.Created ||
                (User.GetUserId() != ideaRecord.AuthorId && !User.IsInRole("Administrator")))
            {
                return RedirectToAction("Index");
            }
            if (ModelState.IsValid)
            {
                ideaRecord.EditorId = User.GetUserId();
                ideaRecord.EditeDate = DateTime.Now;
                ideaRecord.Prewiew = IdeaRecord.GetPrewiew(ideaRecord.Text);
                _context.Update(ideaRecord);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ideaRecord);
        }

        // GET: IdeaRecords/Delete/5
        [ActionName("Delete")]
        [Authorize]
        public IActionResult Delete(string id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            IdeaRecord ideaRecord = _context.IdeaRecord.Single(m => m.ID == id);
            if (ideaRecord == null)
            {
                return HttpNotFound();
            }
            if (ideaRecord.Status == IdeaRecordStatusEnum.ArchiveByUser ||
                (User.GetUserId() != ideaRecord.AuthorId && !User.IsInRole("Administrator")))
            {
                return RedirectToAction("Index");
            }
            return View(ideaRecord);
        }

        // POST: IdeaRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult DeleteConfirmed(string id)
        {
            var ideaRecord = _context.IdeaRecord.Single(m => m.ID == id);
            if (ideaRecord.Status == IdeaRecordStatusEnum.ArchiveByUser)
            {
                return RedirectToAction("Index");
            }

            if (User.GetUserId() == ideaRecord.AuthorId)
            {
                if (User.IsInRole("Administrator"))
                {
                    SetStatus(ideaRecord);
                }
                else
                {
                    switch (ideaRecord.Status)
                    {
                        case IdeaRecordStatusEnum.Created:
                            SetStatus(ideaRecord, IdeaRecordStatusEnum.RemovedByUser);
                            break;
                        case IdeaRecordStatusEnum.RemovedByAdmin:
                            SetStatus(ideaRecord);
                            return RedirectToAction("RemoveRequest");
                    }
                }
            }
            else if (User.IsInRole("Administrator"))
            {
                switch (ideaRecord.Status)
                {
                    case IdeaRecordStatusEnum.Created:
                        SetStatus(ideaRecord, IdeaRecordStatusEnum.RemovedByAdmin);
                        break;
                    case IdeaRecordStatusEnum.RemovedByUser:
                        SetStatus(ideaRecord);
                        return RedirectToAction("RemoveRequest");
                    case IdeaRecordStatusEnum.ArchiveByAdmin:
                        SetStatus(ideaRecord);
                        return RedirectToAction("ArchiveRecords");
                }
            }

            return RedirectToAction("Index");
        }

        // GET: IdeaRecords
        [Authorize]
        public IActionResult RemoveRequest()
        {
            IQueryable<IdeaRecord> applicationDbContext =
                _context.IdeaRecord.Include(i => i.Author).Include(i => i.Editor);
            if (User.IsInRole("Administrator"))
            {
                applicationDbContext = applicationDbContext.Where(l => l.Status == IdeaRecordStatusEnum.RemovedByUser);
            }
            else
            {
                applicationDbContext =
                    applicationDbContext.Where(
                        l => l.AuthorId == User.GetUserId() && l.Status == IdeaRecordStatusEnum.RemovedByAdmin);
            }
            return View(applicationDbContext.ToList());
        }

        // GET: IdeaRecords
        [Authorize]
        public IActionResult ArchiveRecords()
        {
            if (!User.IsInRole("Administrator"))
            {
                return RedirectToAction("Index");
            }

            var applicationDbContext = _context.IdeaRecord.Include(i => i.Author)
                    .Where(l => l.Status == IdeaRecordStatusEnum.ArchiveByAdmin || l.Status == IdeaRecordStatusEnum.ArchiveByUser);
            return View(applicationDbContext.ToList());

        }

        // GET: IdeaRecords
        [Authorize]
        public IActionResult Restore(string id)
        {
            if (User.IsInRole("Administrator"))
            {
                var ideaRecord = _context.IdeaRecord.Single(m => m.ID == id);
                if (ideaRecord.Status == IdeaRecordStatusEnum.RemovedByUser)
                {
                    SetStatus(ideaRecord, IdeaRecordStatusEnum.Created);
                    return RedirectToAction("RemoveRequest");
                }
                if (ideaRecord.Status == IdeaRecordStatusEnum.ArchiveByUser)
                {
                    SetStatus(ideaRecord, IdeaRecordStatusEnum.Created);
                    return RedirectToAction("ArchiveRecords");
                }
            }
            return RedirectToAction("Index");
        }

        // POST: IdeaRecords/Restore/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Archive(string id)
        {
            var ideaRecord = _context.IdeaRecord.Single(m => m.ID == id);
            if (User.IsInRole("Administrator") && ideaRecord.Status == IdeaRecordStatusEnum.RemovedByUser)
            {
                SetStatus(ideaRecord, IdeaRecordStatusEnum.ArchiveByAdmin);
            }
            if (User.GetUserId() == ideaRecord.AuthorId && ideaRecord.Status == IdeaRecordStatusEnum.RemovedByAdmin)
            {
                SetStatus(ideaRecord, IdeaRecordStatusEnum.ArchiveByUser);
            }
            return RedirectToAction("RemoveRequest");
        }

        [MultipleButtonsAttribute(typeof (ButtonType))]
        public IActionResult Manage(ButtonType button, string id)
        {
            switch (button)
            {
                case ButtonType.Delete:
                    return RedirectToAction("Delete", new { id = id });
                case ButtonType.Restore:
                    return Restore(id);
                case ButtonType.Archive:
                    return Archive(id);
            }

            return RedirectToAction("Index", "Home");
        }

        private void SetStatus(IdeaRecord ideaRecord, IdeaRecordStatusEnum? status = null)
        {
            if (status != null)
            {
                ideaRecord.Status = (IdeaRecordStatusEnum) status;
                _context.Update(ideaRecord);
                _context.SaveChanges();
            }
            else
            {
                _context.IdeaRecord.Remove(ideaRecord);
                _context.SaveChanges();
            }
        }
    }
}
