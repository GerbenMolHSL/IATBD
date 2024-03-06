function validateForm(form) {
    if (!form) {
        console.error('Form not provided.');
        return;
    }

    let hasError = false;

    // Loop through each input element in the form
    Array.from(form.elements).forEach(input => {
        let parent = input.parentElement;
        if (input.dataset.required && input.value.trim() === '') {
            hasError = true;
            parent.classList.add('error');
        } else {
            parent.classList.remove('error');
        }
    });

    if (hasError) {
        // Prevent the form from submitting
        console.log('Form submission cancelled due to validation errors.');
        return false;
    } else {
        // Proceed with form submission
        console.log('Form submitted successfully.');
        return true;
    }
}