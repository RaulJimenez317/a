document.addEventListener("DOMContentLoaded", () => {

    const searchUser = document.getElementById('searchUser');
    if (searchUser) {
        searchUser.addEventListener('input', () => {
            const val = searchUser.value.trim().toLowerCase();
            document.querySelectorAll('.user-card').forEach(card => {
                const name = (card.dataset.name || '').trim().toLowerCase();
                card.style.display = name.includes(val) ? '' : 'none';
            });
        });
    }
    document.querySelectorAll('.user-card').forEach(card => {
        card.addEventListener('click', () => {
            const userId = card.dataset.userId;
            window.location = `/AdminHome/AdminManageUser?UserId=${userId}`;
        });
    });

    window.openDeleteModal = (userId, email) => {
        const modal = document.getElementById("confirmDeleteModal");
        modal.classList.remove("hidden");
        document.getElementById("deleteUserId").value = userId;
        document.getElementById("userToDeleteEmail").innerText = email;
    }

    window.closeDeleteModal = () => {
        const modal = document.getElementById("confirmDeleteModal");
        if (modal) modal.classList.add("hidden");
    }

    const closeModalBtn = document.getElementById("closeModalBtn");
    if (closeModalBtn) {
        closeModalBtn.addEventListener("click", () => {
            window.closeDeleteModal();
        });
    }

    window.showSection = (sectionId, event) => {
        document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
        document.querySelectorAll('.tab-button').forEach(btn => btn.classList.remove('active'));

        const section = document.getElementById(sectionId);
        if (section) section.classList.add('active');
        if (event) event.currentTarget.classList.add('active');
    }

    document.querySelectorAll('.dropbtn').forEach(btn => {
        btn.addEventListener('click', e => {
            e.stopPropagation();
            const dropdown = btn.nextElementSibling;
            document.querySelectorAll('.dropdown-content').forEach(dc => {
                if (dc !== dropdown) dc.classList.remove('show');
            });
            dropdown.classList.toggle('show');
        });
    });

    document.addEventListener('click', () => {
        document.querySelectorAll('.dropdown-content').forEach(dc => dc.classList.remove('show'));
    });


    const searchSubscribed = document.getElementById('searchSubscribed');
    if (searchSubscribed) {
        searchSubscribed.addEventListener('input', () => {
            const filter = searchSubscribed.value.toLowerCase();
            document.querySelectorAll('#inscritos .project-card').forEach(p => {
                const name = p.querySelector('a')?.textContent.toLowerCase() || '';
                p.style.display = name.includes(filter) ? '' : 'none';
            });
        });
    }

    const searchCreated = document.getElementById('searchCreated');
    if (searchCreated) {
        searchCreated.addEventListener('input', () => {
            const filter = searchCreated.value.toLowerCase();
            document.querySelectorAll('#creados .project-card').forEach(p => {
                const name = p.querySelector('a')?.textContent.toLowerCase() || '';
                p.style.display = name.includes(filter) ? '' : 'none';
            });
        });
    }


});

