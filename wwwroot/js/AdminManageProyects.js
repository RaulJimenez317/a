function renderCategoryChart(labels, data) {
    const ctx = document.getElementById('categoryChart');
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Proyectos por Categoría',
                data: data,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
}


document.getElementById("btnAddCategory").addEventListener("click", () => {
    new bootstrap.Modal(document.getElementById("addCategoryModal")).show();
});

document.getElementById("saveCategoryBtn").addEventListener("click", async () => {
    const name = document.getElementById("categoryName").value.trim();
    const projectID = document.getElementById("projectSelect").value;

    if (!name) return alert("Ingrese un nombre.");

    const response = await fetch('?handler=AddCategory', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, projectID: parseInt(projectID) })
    });

    if (response.ok) {
        const updated = await response.json();
        renderCategoryChart(updated);
        location.reload();
    } else {
        alert("Error al guardar categoría");
    }
});
