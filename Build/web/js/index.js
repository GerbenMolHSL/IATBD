function validateForm(form) {
    if (!form) {
        console.error('Form not provided.');
        return;
    }

    let hasError = false;

    // Loop through each input element in the form
    Array.from(form.elements).forEach(input => {
        let parent = input.parentElement;
        var isRequired = /^true$/i.test(input.dataset.required);
        if (isRequired && input.value.trim() === '') {
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

window.onload = () => {
    // Initialize the file upload inputs
    Array.from(document.querySelectorAll('input[type="file"]')).forEach(input => {
        // Finding label based on name attribute
        let inputDiv = input.parentElement;
        input.addEventListener('change', (event) => {            
            // Start upload with fetch API and add new file to the formData
            
            if(input.files.length === 0) return;
            
            let formData = new FormData();
            formData.append('file', input.files[0]);
            formData.append('table', inputDiv.dataset.table);
            
            fetch('/upload', {
                method: 'POST',
                body: formData
            }).then(response => {
                if (response.ok) {
                    // JSON returned contains ID for deleting and downloading parse it and add it to the inputDiv
                    response.json().then(data => {
                        var id = data.id;
                        console.log(id);

                        inputDiv.appendChild(createContentDiv(id, input.files[0].name, input.Name));
                        
                        // Add the id to input value with type text that is direct child of inputDiv
                        let childInput = inputDiv.querySelector('input[type="text"]');
                        childInput.value = childInput.value + id+";";
                    });
                } else {
                    throw new Error('An error occurred while uploading the file.');
                }
            }).then(data => {
                console.log(data);
            }).catch(error => {
                console.error(error);
            });
        });
    });
}

function createContentDiv(id, name, columnName){
    let contentDiv = document.createElement('div');
    contentDiv.classList.add('content');
    contentDiv.dataset.id = id;
    let textInput = document.createElement('input');
    textInput.type = 'text';
    textInput.classList.add('disabled-text');
    textInput.value = name;
    textInput.disabled = true;
    let buttonsDiv = document.createElement('div');
    buttonsDiv.classList.add('buttons');
    let deleteButton = document.createElement('button');
    deleteButton.type = 'button';
    deleteButton.classList.add('btn', 'delete-btn');
    deleteButton.innerHTML = '<i class="fas fa-trash-alt"></i>';
    deleteButton.setAttribute("onclick", `deleteFile('${id}','${columnName}')`);
    let downloadButton = document.createElement('button');
    downloadButton.type = 'button';
    downloadButton.classList.add('btn', 'download-btn');
    downloadButton.innerHTML = '<i class="fas fa-download"></i>';
    downloadButton.setAttribute("onclick", `downloadFile('${id}')`);
    let viewButton = document.createElement('button');
    viewButton.type = 'button';
    viewButton.classList.add('btn', 'view-btn');
    viewButton.innerHTML = '<i class="fas fa-magnifying-glass"></i>';
    viewButton.setAttribute("onclick", `viewFile('${id}')`);
    buttonsDiv.appendChild(deleteButton);
    buttonsDiv.appendChild(downloadButton);
    buttonsDiv.appendChild(viewButton);
    contentDiv.appendChild(textInput);
    contentDiv.appendChild(buttonsDiv);
    
    return contentDiv;
}

// Start upload process
function uploadFile(button){
    let input = button.closest('.FileUpload').querySelector('input[type="file"]');
    // Set max files to 10
    if(input.files.length >= 10){
        alert('You can only upload 10 files at a time.');
        return;
    }
    input.click();
}

// Initiate file select
function initiateFileSelect(id){
    let completeBlock = document.getElementById(id);
    let searchInput = completeBlock.querySelector('input[type="text"]');
    let files = completeBlock.querySelector('.files');
    let selectButton = completeBlock.querySelector('.button');
    searchInput.addEventListener('keyup', function(event){
        let searchValue = event.target.value;
        // Create formdate  with searchValue in search and alredy selected files in files
        let formData = new FormData();
        formData.append('search', searchValue);
        let fileInputID = id.replace("FileSelect", "");        
        let selectedFiles = document.getElementsByName(fileInputID)[0].value;
        formData.append('files', selectedFiles);
        // Create request to /fileSearch
        
        fetch('/searchFiles', {
            method: 'POST',
            body: formData
        }).then(response => {
            if (response.ok) {
                // Raw html returned placed in the files block
                response.text().then(data => {
                    let div = document.createElement('div');
                    div.classList.add('files');
                    div.innerHTML = data;
                    $(`#${id} .files`)[0].replaceWith($('<div class="files">' + data + "</div>")[0]);
                    files = completeBlock.querySelector('.files');
                });
            } else {
                throw new Error('An error occurred while uploading the file.');
            }
        }).then(data => {
            console.log(data);
        }).catch(error => {
            console.error(error);
        });
    })
    
    selectButton.addEventListener('click', function(){
        let fileInputID = id.replace("FileSelect", "");
        let fileInput = document.getElementsByName(fileInputID)[0];
        let selectedFiles = files.querySelectorAll('input[type="checkbox"]:checked');
        let selectedIds = fileInput.value;
        selectedFiles.forEach(file => {
            if(!fileInput.value.split(";").includes(file.dataset.id)){
                selectedIds += file.dataset.id + ";";
                fileInput.parentElement.appendChild(createContentDiv(file.dataset.id, file.dataset.name, fileInputID));
            }
            selectedFiles.checked = false;
        });
        fileInput.value = selectedIds;
    });
}

function deleteFile(id, inputName){
    if(!confirm("Weet je zeker dat je dit bestand wilt verwijderen?\nDit betekent niet dat hij van je bestandsopslag verwijdert word")) return;
    let input = document.querySelector(`.FileUpload input[name="${inputName}"]`);
    // Split value and remove id from it
    let ids = input.value.split(";");
    ids = ids.filter(value => value !== id);
    input.value = ids.join(";");
    
    let contentDiv = document.querySelector(`.FileUpload .content[data-id="${id}"]`);
    contentDiv.remove();
}

function viewFile(id, columnName){
    var fileViewDiv = document.querySelector("#FileViewDiv");
    var fileView = fileViewDiv.querySelector("#fileView");
    
    fileView.src = `/view?id=${id}`;
    $('#FileViewDiv.ui.modal').modal('show')
}

function downloadFile(id, columnName){
    // Create an anchor element
    var anchor = document.createElement('a');

    // Set the href attribute to the file URL
    anchor.href = "/download?id=" + id;

    // Programmatically trigger a click event on the anchor element
    // This will prompt the browser to download the file
    anchor.click();
}

function changeTabItem(event,newTabName){
    let target = event.target;
    let tabContainer = target.closest(".tabMenu");
    let tabItems = tabContainer.querySelectorAll("div[name]");
    tabItems.forEach((e) => {
        if(e.getAttribute("name") === newTabName) e.classList.add("active");
        else e.classList.remove("active");
    });
}