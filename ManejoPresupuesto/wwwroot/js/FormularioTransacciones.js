function inicializarFormularioTransacciones(urlObtenerCategorias) {
	$("#TipoOperacionId").change(async function () {
		const valorSeleccionado = parseInt($(this).val());
		const respuesta = await fetch(urlObtenerCategorias, {
			method: 'POST',
			body: JSON.stringify(valorSeleccionado),
			headers: {
				'Content-Type': 'application/json'
			}
		});
		const json = await respuesta.json();
		const opciones = json.map(categoria =>
			`<option value="${categoria.value}">${categoria.text}</option>`
		);
		$("#CategoriaId").html(opciones);
	});
}