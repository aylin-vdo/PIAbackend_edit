﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIA_BackEnd.DTOs;
using PIA_BackEnd.Entidades;

namespace PIA_BackEnd.Controllers
{
    [ApiController]
    [Route("usuario")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class UsuarioController : ControllerBase
    {
        private readonly ApplicationDBContext dbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration configuration;

        public UsuarioController(ApplicationDBContext dbContext, IMapper mapper, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            _mapper = mapper;
            this.configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<GetUsuarioDTO>>> Get()
        {
            var usuarios = await dbContext.Usuario.ToListAsync();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Usuario, GetUsuarioDTO>();
            });

            var mapper = new Mapper(config);
            var usuariosDTO = mapper.Map<List<GetUsuarioDTO>>(usuarios);

            return usuariosDTO;
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Post([FromBody] UsuarioDTO usuarioDto)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<UsuarioDTO, Usuario>();
                cfg.CreateMap<Usuario, GetUsuarioDTO>();
            });

            var mapper = new Mapper(config);
            var existeUsuarioMismoNombre = await dbContext.Usuario.AnyAsync(x => x.Nombre == usuarioDto.Nombre);

            if (existeUsuarioMismoNombre)
            {
                return BadRequest($"Ya existe un usuario con el nombre {usuarioDto.Nombre}");
            }

            var usuario = mapper.Map<Usuario>(usuarioDto);

            dbContext.Add(usuario);
            await dbContext.SaveChangesAsync();

            var usuarioDTO = mapper.Map<GetUsuarioDTO>(usuario);

            return CreatedAtRoute("obtenerusuario", new { id = usuario.Id }, usuarioDTO);
        }
    }
}