using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using BestStoreApi.Models;
using BestStoreApi.Models.Dto;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BestStoreApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        // private readonly List<string> listSubjects = new List<string>()
        // {
        //     "Order Status", "Refund Requet", "Job Application", "Other"
        // };

        private readonly EmailSender emailSender;
        public ContactController(ApplicationDbContext context, EmailSender emailSender)
        {
            this.context = context;
            this.emailSender = emailSender;
        }

        [HttpGet("Subjects")]
        public ActionResult GetSubjects()
        {
            var listSubjects = context.Subjects.ToList();
            return Ok(listSubjects);
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult GetContacts(int? page)
        {
            if (page == null || page < 1)
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            decimal count = context.Contacts.Count();

            totalPages = (int)Math.Ceiling(count / pageSize);

            var skipedRowCount = (int)(page! - 1) * pageSize;

            var contacts = context.Contacts
                .Include(c => c.Subject)
                .OrderBy(c => c.Id)
                .Skip(skipedRowCount)
                .Take(pageSize)
                .ToList();

            var response = new
            {
                Contacts = contacts,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };
            return Ok(response);
            // return Ok(contacts);
        }

        [Authorize(Roles = "admin")]
        [HttpGet("{id}")]
        public ActionResult GetContact(int id)
        {
            // var contact = context.Contacts.Find(id);
            var contact = context.Contacts.Include(c => c.Subject).FirstOrDefault(r => r.Id == id);

            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [HttpPost]
        // public ActionResult CreateContact(ContactDto contactDto)
        public async Task<IActionResult> CreateContact(ContactDto contactDto)

        {
            var Subject = context.Subjects.Find(contactDto.SubjectId);
            // if (!listSubjects.Contains(contactDto.Subject))
            if (Subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a Subject from the Subjects list");
                return BadRequest(ModelState);
            }

            Contact contact = new Contact()
            {
                FirstName = contactDto.FirstName,
                LastName = contactDto.LastName,
                Email = contactDto.Email,
                Phone = contactDto.Phone,
                // Subject = contactDto.Subject,
                Subject = Subject,
                Message = contactDto.Message,
                CreatedAt = DateTime.Now
            };
            context.Contacts.Add(contact);
            context.SaveChanges();

            // send confirmation mail
            string emailSubject = "Contact Confirmation";
            string username = contactDto.FirstName + " " + contactDto.LastName;
            string emailMessage = "Dear " + username + "\n" +
                "We received you message. Thank you for contacting us.\n" +
                "Our team will contact you very soon.\n" +
                "Best Regards\n\n" +
                "Your Message:\n" + contactDto.Message;

            await emailSender.SendEmailAsync(contact.Email, emailSubject, emailMessage);
            // emailSender.SendEmail(emailSubject, contact.Email, username, emailMessage).Wait();

            return Ok(contact);
        }

        [HttpPut("id")]
        public IActionResult UpdateContact(int id, ContactDto contactDto)
        {
            var Subject = context.Subjects.Find(contactDto.SubjectId);
            // if (!listSubjects.Contains(contactDto.Subject))
            if (Subject == null)
            {
                ModelState.AddModelError("Subject", "Please select a Subject from the Subjects list");
                return BadRequest(ModelState);
            }

            var contact = context.Contacts.Find(id);
            if (contact == null)
            {
                return NotFound();
            }

            contact.FirstName = contactDto.FirstName;
            contact.LastName = contactDto.LastName;
            contact.Email = contactDto.Email;
            contact.Phone = contactDto.Phone;
            // contact.Subject = contactDto.Subject;
            contact.Subject = Subject;
            contact.Message = contactDto.Message;
            // context.Contacts.Update(contact);
            context.SaveChanges();

            return Ok(contact);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("id")]
        public IActionResult DeleteContact(int id)
        {
            var client = new SmtpClient()
            {

            };
            // here we need to access the contacts table 2 times to delete the contact
            // var contact = context.Contacts.Find(id);
            // if (contact == null)
            // {
            //     return NotFound();
            // }

            // context.Contacts.Remove(contact);
            // context.SaveChanges();

            // in this way we access to contacts table one time

            try
            {
                var contact = new Contact() { Id = id, Subject = new Subject() };
                context.Contacts.Remove(contact);
                context.SaveChanges();
            }
            catch (System.Exception)
            {
                return NotFound();
            }

            return Ok();
            // return NoContent();
        }
    }
}