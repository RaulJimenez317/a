const photosInput = document.getElementById("photosInput");
const filesInput = document.getElementById("filesInput");
const preview = document.getElementById("preview");

let accumulatedPhotos = [];
let accumulatedFiles = [];

function renderPreview() {
    preview.innerHTML = "";
    accumulatedPhotos.forEach((file, idx) => {
        const container = document.createElement("div");
        container.className = "position-relative";
        container.innerHTML = `
                <img src="${URL.createObjectURL(file)}" style="max-width:80px;" class="rounded shadow-sm me-2 mb-2">
                <span class="position-absolute top-0 end-0 bg-danger text-white rounded-circle px-1 fw-bold" style="cursor:pointer;">&times;</span>
            `;
        container.querySelector("span").onclick = () => {
            accumulatedPhotos.splice(idx, 1);
            renderPreview();
        };
        preview.appendChild(container);
    });

    accumulatedFiles.forEach((file, idx) => {
        const container = document.createElement("div");
        container.className = "position-relative border rounded p-1 me-2 mb-2";
        container.innerHTML = `
                <i class="bi bi-file-earmark"></i> ${file.name}
                <span class="position-absolute top-0 end-0 bg-danger text-white rounded-circle px-1 fw-bold" style="cursor:pointer;">&times;</span>
            `;
        container.querySelector("span").onclick = () => {
            accumulatedFiles.splice(idx, 1);
            renderPreview();
        };
        preview.appendChild(container);
    });
}

photosInput.addEventListener("change", e => {
    accumulatedPhotos.push(...e.target.files);
    renderPreview();
    photosInput.value = "";
});

filesInput.addEventListener("change", e => {
    accumulatedFiles.push(...e.target.files);
    renderPreview();
    filesInput.value = "";
});

document.getElementById("commentForm").addEventListener("submit", function (e) {
    const appendFiles = (files, name) => {
        if (files.length === 0) return;
        const dt = new DataTransfer();
        files.forEach(f => dt.items.add(f));
        const input = document.createElement("input");
        input.type = "file";
        input.name = name;
        input.files = dt.files;
        input.multiple = true;
        input.style.display = "none";
        this.appendChild(input);
    };
    appendFiles(accumulatedPhotos, "PhotosAttached");
    appendFiles(accumulatedFiles, "FilesAttached");
});


