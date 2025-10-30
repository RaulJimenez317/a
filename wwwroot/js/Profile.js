function showSection(sectionId) {
    document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
    document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));
    document.getElementById(sectionId).classList.add('active');
    event.target.classList.add('active');
}

function showEditForm() {
    document.getElementById('editProfileForm').style.display = 'block';
    document.getElementById('editProfileBtn').style.display = 'none';
    document.getElementById('aboutText').style.display = 'none';
}

function hideEditForm() {
    document.getElementById('editProfileForm').style.display = 'none';
    document.getElementById('editProfileBtn').style.display = 'inline-block';
    document.getElementById('aboutText').style.display = 'block';
}

const searchInput = document.getElementById('searchCreated');
const projects = document.querySelectorAll('.project-card');

searchInput.addEventListener('input', function () {
    const filter = this.value.toLowerCase();

    projects.forEach(project => {
        const nameEl = project.querySelector('.project-name');
        if (!nameEl) return; 

        const name = nameEl.textContent.toLowerCase();
        project.style.display = name.includes(filter) ? '' : 'none';
    });
});

const searchSubscribed = document.getElementById('searchSubscribed');
const subscribedProjects = document.querySelectorAll('#inscritos .project-card');

searchSubscribed.addEventListener('input', function () {
    const filter = this.value.toLowerCase();

    subscribedProjects.forEach(project => {
        const nameEl = project.querySelector('.project-name');
        if (!nameEl) return;
        const name = nameEl.textContent.toLowerCase();
        project.style.display = name.includes(filter) ? '' : 'none';
    });
});


function previewPhoto(input) {
    const file = input.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = e => {
            document.getElementById('profilePreview').src = e.target.result;
            document.getElementById('removeProfileImage').value = 'false';
        }
        reader.readAsDataURL(file);
    }
}

function removePhoto() {
    document.getElementById('profilePreview').src = '/images/default-user.png';
    document.getElementById('removeProfileImage').value = 'true';
    document.querySelector('input[name="ProfileImage"]').value = '';
}

function removeCV() {
    const cvPreview = document.getElementById('cvPreview');
    if (cvPreview) cvPreview.style.display = 'none'; 
    document.getElementById('removeCurriculum').value = 'true'; 
    document.querySelector('input[name="ProfileCurriculum"]').value = '';
}

function previewCV(input) {
    if (input.files && input.files[0]) {
        const fileName = input.files[0].name;
        const cvPreview = document.getElementById('cvPreview');
        cvPreview.style.display = 'flex';
        const link = cvPreview.querySelector('a.cv-link');
        if (link) {
            link.textContent = fileName;
            link.href = URL.createObjectURL(input.files[0]);
        }
    }
}