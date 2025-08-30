using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.DTOs.Request;
using Models.DTOs.Response;
using Repositories.IRepository;

namespace PetHospitalApi.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IServiceRepository _serviceRepository;
        private readonly IVetRepository _vetRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly UserManager<User> _userManager;
        private readonly IPetRepository _petRepository;

        public ReservationController(IServiceRepository serviceRepository, IVetRepository vetRepository, IAppointmentRepository appointmentRepository, UserManager<User> userManager, IPetRepository petRepository)
        {
            _serviceRepository = serviceRepository;
            _vetRepository = vetRepository;
            _appointmentRepository = appointmentRepository;
            _userManager = userManager;
            _petRepository = petRepository;
        }
        [HttpGet("GetAllServices")]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _serviceRepository.GetAsync();
            if (services == null)
            {
                return NotFound("No services found.");
            }

            return Ok(services.Adapt<IEnumerable<ServiceResponse>>());
        }
        [HttpGet("GetAllVets")]
        public async Task<IActionResult> GetAllVets()
        {
            var vets = await _vetRepository.GetAsync();
            if (vets == null)
            {
                return NotFound("No vets found.");
            }

            return Ok(vets.Adapt<IEnumerable<VetResponse>>());
        }
        [HttpGet("GetVetById/{id}")]
        public async Task<IActionResult> GetVetById([FromRoute] string id)
        {
            var vet = await _vetRepository.GetOneAsync(e => e.VetId == id);
            if (vet == null)
            {
                return NotFound($"Vet with ID {id} not found.");
            }
            return Ok(vet.Adapt<VetResponse>());

        }
        [HttpGet("GetServiceById/{id}")]
        public async Task<IActionResult> GetServiceById([FromRoute] int id)
        {
            var service = await _serviceRepository.GetOneAsync(e => e.ServiceId == id);
            if (service == null)
            {
                return NotFound($"Service with ID {id} not found.");
            }
            return Ok(service.Adapt<ServiceResponse>());
        }
        [HttpPost("CreateReservation")]
        public async Task<IActionResult> CreateReservation([FromBody] AppointmentRequest AppointmentRequest)
        {
            if (AppointmentRequest == null)
            {
                return BadRequest("Invalid appointment request.");
            }
            var appointment = AppointmentRequest.Adapt<Appointment>();
            // Validate the vet exists  
            var vet = await _vetRepository.GetOneAsync(v => v.VetId == appointment.VetId);
            if (vet == null)
            {
                return NotFound($"Vet with ID {appointment.VetId} not found.");
            }
            // Validate the pet exists
            var pet = await _petRepository.GetOneAsync(p => p.PetId == appointment.PetId);
            if (pet == null)
            {
                return NotFound($"Pet with ID {appointment.PetId} not found.");
            }
            // Validate the user exists
            var user = await _userManager.FindByIdAsync(appointment.UserId);

            if (user == null)
            {
                return NotFound($"User with ID {appointment.UserId} not found.");
            }

            var result = await _appointmentRepository.CreateAsync(appointment);
            if (result == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating reservation.");
            }
            return CreatedAtAction(nameof(GetVetById), new { id = appointment.VetId }, appointment.Adapt<AppointmentResponse>());



        }
        [HttpGet("GetAllReservations/{userid}")]
        public async Task<IActionResult> GetAllReservations([FromRoute]string userid)
        {
            var appointments = await _appointmentRepository.GetAsync(
                a => a.UserId.Trim().ToLower() == userid.Trim().ToLower()
            );
            if (appointments == null || !appointments.Any())
            {
                return NotFound("No reservations found.");
            }
            Console.WriteLine($"FromRoute: {userid}");
            foreach (var a in appointments)
            {
                Console.WriteLine($"FromDB: {a.UserId}");
            }

            return Ok(appointments.Adapt<IEnumerable<AppointmentResponse>>());
        }
        [HttpGet("GetReservationById/{id}")]
        public async Task<IActionResult> GetReservationById([FromRoute] int id)
        {
            var appointment = await _appointmentRepository.GetOneAsync(a => a.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
            return Ok(appointment.Adapt<AppointmentResponse>());
        }
        [HttpPut("UpdateReservation/{id}")]
        public async Task<IActionResult> UpdateReservation([FromRoute] int id, [FromBody] AppointmentRequest appointmentRequest)
        {
            if (appointmentRequest == null)
            {
                return BadRequest("Invalid appointment request.");
            }
            var appointment = await _appointmentRepository.GetOneAsync(a => a.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
            appointment = appointmentRequest.Adapt(appointment);
            var updatedAppointment = await _appointmentRepository.EditAsync(appointment);
            if (updatedAppointment == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating reservation.");
            }
            return Ok(updatedAppointment.Adapt<AppointmentResponse>());
        }
        [HttpDelete("DeleteReservation/{id}")]
        public async Task<IActionResult> DeleteReservation([FromRoute] int id)
        {
            var appointment = await _appointmentRepository.GetOneAsync(a => a.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }
             await _appointmentRepository.DeleteAsync(appointment);
            
            return NoContent();
        }
    }
    }
