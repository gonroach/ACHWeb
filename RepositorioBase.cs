using Datos.Persistencia.Core;
using Dominio.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Utilitarios.Repositorios
{
    public class RepositorioBase<T> : IRepositorioBase<T> where T : class
    {
        #region Miembros

        /// <summary>
        /// Unidad de Trabajo para contexto de entidades base
        /// </summary>
        readonly IContextoUnidadDeTrabajo _unidadDeTrabajo;

        #endregion

        #region Atributos

        /// <summary>
        /// Propiedad de sólo lectura para Unidad de trabajo de contexto
        /// </summary>
        public IUnidadDeTrabajo UnidadDeTrabajo
        {
            get { return _unidadDeTrabajo; }
        }

        /// <summary>
        /// Propiedad que contiene el identificador del usuario que realiza la operación sobre la BD
        /// </summary>
        public string Usuario { get; set; }

        #endregion

        #region Constructor
        
        /// <summary>
        /// Inicializa una instancia de Repositorio
        /// </summary>
        /// <param name="unidadDeTrabajo"></param>
        public RepositorioBase(IContextoUnidadDeTrabajo unidadDeTrabajo)
        {
            _unidadDeTrabajo = unidadDeTrabajo;
        }

        #endregion

        #region Métodos Base

        /// <summary>
        /// Obtiene listado de objetos a partir de parámetros de búsqueda y restricciones de ordenamiento
        /// </summary>
        /// <param name="filtro">Expresión lambda que define filtros de búsqueda</param>
        /// <param name="ordenarPor">Expresión que define ordenamiento</param>
        /// <param name="propiedadesAIncluir">Cadena que define las propiedades a incluir en la búsqueda asociada</param>
        /// <returns>Listado de objetos asociados a la búsqueda</returns>
        public IEnumerable<T> Obtener(Expression<Func<T, bool>> filtro = null, Func<IQueryable<T>, IOrderedQueryable<T>> ordenarPor = null, string propiedadesAIncluir = "")
        {
            IQueryable<T> consulta = _unidadDeTrabajo.Set<T>();

            if (filtro != null)
            {
                consulta = consulta.Where(filtro);
            }

            if (!string.IsNullOrEmpty(propiedadesAIncluir))
            {
                consulta = propiedadesAIncluir.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).
                    Aggregate(consulta, (current, propiedadIncluida) => current.Include(propiedadIncluida));
            }

            return ordenarPor != null ? ordenarPor(consulta).ToList() : consulta.ToList();
        }

        /// <summary>
        /// Obtiene listado de objetos a partir de parámetros de búsqueda y restricciones de ordenamiento
        /// </summary>
        /// <param name="filtro">Expresión lambda que define filtros de búsqueda</param>
        /// <param name="ordenarPor">Expresión que define ordenamiento</param>
        /// <param name="propiedadesAIncluir">Cadena que define las propiedades a incluir en la búsqueda asociada</param>
        /// <returns>Listado de objetos asociados a la búsqueda</returns>
        public IEnumerable<T> ObtenerNoTracking(Expression<Func<T, bool>> filtro = null, Func<IQueryable<T>, IOrderedQueryable<T>> ordenarPor = null, string propiedadesAIncluir = "")
        {
            IQueryable<T> consulta = _unidadDeTrabajo.Set<T>();

            if (filtro != null)
            {
                consulta = consulta.AsNoTracking().Where(filtro);
            }

            if (!string.IsNullOrEmpty(propiedadesAIncluir))
            {
                consulta = propiedadesAIncluir.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                    Aggregate(consulta, (current, propiedadIncluida) => current.Include(propiedadIncluida));
            }

            return ordenarPor != null ? ordenarPor(consulta).ToList() : consulta.ToList();
        }

        /// <summary>
        /// Obtiene listado de objetos a partir de un filtro de búsqueda
        /// </summary>
        /// <param name="filtro">Expresión lambda que define filtros de búsqueda</param>
        /// <returns>Listado de objetos asociados a la búsqueda</returns>
        public IEnumerable<T> Obtener(Expression<Func<T, bool>> filtro)
        {
            return _unidadDeTrabajo.Set<T>().Where(filtro);
        }


        /// <summary>
        /// Obtiene listado de objetos a partir de un filtro de búsqueda
        /// </summary>
        /// <param name="filtro">Expresión lambda que define filtros de búsqueda</param>
        /// <returns>Listado de objetos asociados a la búsqueda</returns>
        public IEnumerable<T> ObtenerNoTracking(Expression<Func<T, bool>> filtro)
        {
            return _unidadDeTrabajo.Set<T>().AsNoTracking().Where(filtro);
        }

        /// <summary>
        /// Obtiene el listado completo de objetos asociados a entidad genérica
        /// </summary>
        /// <returns>Listado de objetos asociados a la búsqueda</returns>
        public IEnumerable<T> ObtenerTodos()
        {
            return _unidadDeTrabajo.Set<T>().ToList();
        }

        /// <summary>
        /// Obtiene el primer elemento encontrado para un filtro de búsqueda determinado
        /// </summary>
        /// <param name="filtro">Expresión lambda que define filtros de búsqueda</param>
        /// <returns>Primer resultado que hace match con el filtro de búsqueda</returns>
        public T ObtenerPrimero(Expression<Func<T, bool>> filtro)
        {
            return _unidadDeTrabajo.Set<T>().FirstOrDefault(filtro);
        }

        /// <summary>
        /// Obtiene el ultimo registro asociado a una lista ordenada dependiendo de la expresión enviada
        /// </summary>
        /// <param name="filtro">filtro asociado a los datos que se desean obtener</param>
        /// <param name="ordenarPor">Expresión que define la columna de tipo int, que se desea ordenar</param>
        /// <returns>Ultimo Elemento de registrado de la entidad</returns>
        public T ObtenerUltimo(Expression<Func<T, bool>> filtro, Expression<Func<T, int>> ordenarPor)
        {
            IQueryable<T> consulta = _unidadDeTrabajo.Set<T>().Where(filtro);
            return consulta.OrderByDescending(ordenarPor).FirstOrDefault();
        }

        /// <summary>
        /// Encuentra un objeto a partir de su identificador
        /// </summary>
        /// <param name="id">Identificador de objeto buscado</param>
        /// <returns>Elemento que hace match con el id de búsqueda</returns>
        public T Encontrar(object id)
        {
            return _unidadDeTrabajo.Set<T>().Find(id);
        }

        /// <summary>
        /// Inserta un nuevo elemento al contexto (Unidad de trabajo)
        /// </summary>
        /// <param name="entidadACrear">Entidad a insertar en el contexto</param>
        public T Crear(T entidadACrear)
        {
            return _unidadDeTrabajo.Set<T>().Add(entidadACrear);
        }

        /// <summary>
        /// Elimina del contexto un objeto a partir de si identificador
        /// </summary>
        /// <param name="id">Identificador del elemento a eliminar</param>
        public void Eliminar(object id)
        {
            T entidadAEliminar = _unidadDeTrabajo.Set<T>().Find(id);
            Eliminar(entidadAEliminar);
        }

        /// <summary>
        /// Elimina un objeto del contexto (Unidad de trabajo)
        /// </summary>
        /// <param name="entidadAEliminar">Objeto a eliminar</param>
        public void Eliminar(T entidadAEliminar)
        {
            _unidadDeTrabajo.Attach(entidadAEliminar);
            _unidadDeTrabajo.Set<T>().Remove(entidadAEliminar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entidadAEliminar"></param>
        public void EliminarLogico(T entidadAEliminar)
        {
            _unidadDeTrabajo.SetModified(entidadAEliminar);
        }

        /// <summary>
        /// Actualiza estado de una entidad del contexto
        /// </summary>
        /// <param name="entidadAActualizar">Objeto a actualizar</param>
        public void Actualizar(T entidadAActualizar)
        {
            _unidadDeTrabajo.SetModified(entidadAActualizar);
        }

        /// <summary>
        /// Actualiza estado de una entidad del contexto deshabilitanto el autodetectchange
        /// </summary>
        /// <param name="entidadAActualizar">Objeto a actualizar</param>
        public void ActualizarAutoDetectChangeDisabled(T entidadAActualizar)
        {
            _unidadDeTrabajo.SetModifiedAutoDetectChangeDisabled(entidadAActualizar);
        }

        /// <summary>
        /// Actualiza estado de un conjunto de entidades del contexto deshabilitanto el autodetectchange
        /// </summary>
        /// <param name="entidadAActualizar">Objeto a actualizar</param>
        public void ActualizarCollectionAutoDetectChangeDisabled(IEnumerable<T> entidadAActualizar)
        {
            _unidadDeTrabajo.SetModifiedCollectionAutoDetectChangeDisabled(entidadAActualizar);
        }

        /// <summary>
        /// Obtiene los elementos paginados a partir de parámetros
        /// </summary>
        /// <typeparam name="TKey">Tipo de entidad asociada a la llave de la paginación</typeparam>
        /// <param name="indiceDePagina">Índice de la paginación</param>
        /// <param name="cantidadDePaginas">Cantidad de páginas a paginar</param>
        /// <param name="ordenarPor">Expresión para ordenamiento</param>
        /// <param name="ascendente">Indica si el ordenamiento es ascendente</param>
        /// <returns>Listado de objetos paginados</returns>
        public IEnumerable<T> ObtenerElementosPaginados<TKey>(int indiceDePagina, int cantidadDePaginas, Expression<Func<T, TKey>> ordenarPor, bool ascendente = true)
        {
            if (indiceDePagina < 1) { indiceDePagina = 1; }

            if (ordenarPor == null)
                throw new ArgumentNullException();

            return (ascendente)
                            ?
                        _unidadDeTrabajo.Set<T>().OrderBy(ordenarPor)
                            .Skip((indiceDePagina - 1) * cantidadDePaginas)
                            .Take(cantidadDePaginas)
                            .ToList()
                            :
                        _unidadDeTrabajo.Set<T>().OrderByDescending(ordenarPor)
                            .Skip((indiceDePagina - 1) * cantidadDePaginas)
                            .Take(cantidadDePaginas)
                            .ToList();
        }

        /// <summary>
        /// Obtiene los elementos paginados a partir de parámetros
        /// </summary>
        /// <typeparam name="TKey">Tipo de entidad asociada a la llave de la paginación</typeparam>
        /// <param name="indiceDePagina">Índice de la paginación</param>
        /// <param name="cantidadDePaginas">Cantidad de páginas a paginar</param>
        /// <param name="ordenarPor">Expresión para ordenamiento</param>
        /// <param name="ascendente">Indica si el ordenamiento es ascendente</param>
        /// <param name="propiedadesAIncluir">Cadena que contiene propiedades a incluir en la búsqueda realizada</param>
        /// <returns>Listado de objetos paginados</returns>
        public IEnumerable<T> ObtenerElementosPaginados<TKey>(int indiceDePagina, int cantidadDePaginas, Expression<Func<T, TKey>> ordenarPor, bool ascendente = true, string propiedadesAIncluir = "")
        {
            IQueryable<T> consulta = _unidadDeTrabajo.Set<T>();

            if (!String.IsNullOrEmpty(propiedadesAIncluir))
            {
                consulta = propiedadesAIncluir.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).
                    Aggregate(consulta, (current, includeProperty) => current.Include(includeProperty));
            }

            if (indiceDePagina < 1) 
            { 
                indiceDePagina = 1; 
            }

            if (ordenarPor == null)
            {
                throw new ArgumentNullException();
            }

            return (ascendente)
                            ?
                        consulta.OrderBy(ordenarPor)
                            .Skip((indiceDePagina - 1) * cantidadDePaginas)
                            .Take(cantidadDePaginas)
                            .ToList()
                            :
                        consulta.OrderByDescending(ordenarPor)
                            .Skip((indiceDePagina - 1) * cantidadDePaginas)
                            .Take(cantidadDePaginas)
                            .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="filtro"></param>
        /// <param name="indiceDePagina"></param>
        /// <param name="cantidadDePaginas"></param>
        /// <param name="ordenarPor"></param>
        /// <param name="ascendente"></param>
        /// <returns></returns>
        public IEnumerable<T> ObtenerElementosPaginados<TKey>(Expression<Func<T, bool>> filtro, int indiceDePagina, int cantidadDePaginas, Expression<Func<T, TKey>> ordenarPor, bool ascendente = true)
        {
            if (indiceDePagina < 1) { indiceDePagina = 1; }

            if (ordenarPor == null)
                throw new ArgumentNullException();

            return (ascendente)
                            ?
                        _unidadDeTrabajo.Set<T>().Where(filtro).OrderBy(ordenarPor)
                            .Skip((indiceDePagina - 1) * cantidadDePaginas)
                            .Take(cantidadDePaginas)
                            .ToList()
                            :
                        _unidadDeTrabajo.Set<T>().Where(filtro).OrderByDescending(ordenarPor)
                            .Skip((indiceDePagina - 1) * cantidadDePaginas)
                            .Take(cantidadDePaginas)
                            .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="filtro"></param>
        /// <param name="indiceDePagina"></param>
        /// <param name="cantidadDePaginas"></param>
        /// <param name="ordenarPor"></param>
        /// <param name="ascendente"></param>
        /// <param name="propiedadesAIncluir"></param>
        /// <returns></returns>
        public IEnumerable<T> ObtenerElementosPaginados<TKey>(Expression<Func<T, bool>> filtro, int indiceDePagina, int cantidadDePaginas, Expression<Func<T, TKey>> ordenarPor, bool ascendente = true, string propiedadesAIncluir = "")
        {
            IQueryable<T> consulta = _unidadDeTrabajo.Set<T>().Where(filtro);

            if (!String.IsNullOrEmpty(propiedadesAIncluir))
            {
                consulta = propiedadesAIncluir.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).
                    Aggregate(consulta, (current, includeProperty) => current.Include(includeProperty));
            }

            if (indiceDePagina < 1)
            {
                indiceDePagina = 1;
            }

            if (ordenarPor == null)
            {
                throw new ArgumentNullException();
            }

            return (ascendente)
                            ?
                        consulta.OrderBy(ordenarPor)
                            .Skip((indiceDePagina - 1) * cantidadDePaginas)
                            .Take(cantidadDePaginas)
                            .ToList()
                            :
                        consulta.OrderByDescending(ordenarPor)
                            .Skip((indiceDePagina - 1) * cantidadDePaginas)
                            .Take(cantidadDePaginas)
                            .ToList();
        }

        /// <summary>
        /// Obtiene listado de objetos a partir de una consulta SQL
        /// </summary>
        /// <param name="consultaSQL">Consulta SQL a evaluar</param>
        /// <param name="parametros">Parámetros asociados a la consulta</param>
        /// <returns>Listado de objetos asociados a la consulta SQL</returns>
        public IEnumerable<T> ObtenerPorConsulta(string consultaSQL, params object[] parametros)
        {
            return _unidadDeTrabajo.ExecuteQuery<T>(consultaSQL, parametros);
        }

        /// <summary>
        /// Ejecuta consulta SQL en base de datos
        /// </summary>
        /// <param name="comandoSQL">Consulta SQL a ejecutar</param>
        /// <param name="parametros">Parámetros asociados a la consulta</param>
        /// <returns>Listado de objetos asociados a la consulta SQL</returns>
        public int EjecutarConsulta(string comandoSQL, params object[] parametros)
        {
            return _unidadDeTrabajo.ExecuteCommand(comandoSQL, parametros);
        }

        #endregion

        #region Dispose

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _unidadDeTrabajo.Dispose();
        }

        #endregion

    }
}
