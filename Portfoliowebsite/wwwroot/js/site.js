(function () {
    const form = document.getElementById('contactForm');

    if (!form) {
        return;
    }

    const hp = document.getElementById('website');
    const status = document.getElementById('liveStatus');
    const fields = [
        {
            input: document.getElementById('Name'),
            error: document.getElementById('nameErr'),
            message: 'Vul je naam in.'
        },
        {
            input: document.getElementById('Email'),
            error: document.getElementById('emailErr'),
            message: 'Vul een geldig e-mailadres in.'
        },
        {
            input: document.getElementById('Subject'),
            error: document.getElementById('subjectErr'),
            message: 'Vul een onderwerp in.'
        },
        {
            input: document.getElementById('Message'),
            error: document.getElementById('msgErr'),
            message: 'Vul een bericht in van minimaal 10 tekens.'
        }
    ];

    const validateField = (field) => {
        if (!field.input || !field.error) {
            return true;
        }

        const isValid = field.input.checkValidity();
        field.error.textContent = isValid ? '' : field.message;
        return isValid;
    };

    fields.forEach((field) => {
        if (!field.input) {
            return;
        }

        field.input.addEventListener('input', () => {
            if (field.input.id === 'Name') {
                field.input.value = field.input.value.replace(/[0-9]/g, '');
            }
            validateField(field);
        });

        field.input.addEventListener('blur', () => {
            validateField(field);
        });
    });

    form.addEventListener('submit', (event) => {
        if (hp && hp.value.trim() !== '') {
            event.preventDefault();
            return;
        }

        const hasInvalidField = fields.some((field) => !validateField(field));

        if (hasInvalidField) {
            event.preventDefault();
            if (status) {
                status.textContent = 'Controleer de gemarkeerde velden in het formulier.';
            }
            return;
        }

        if (status) {
            status.textContent = 'Je bericht wordt verzonden.';
        }
    });
})();
