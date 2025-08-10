// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/*if ('serviceWorker' in navigator) {
	navigator.serviceWorker.register('/service-worker.js')
		.then(function (registration) {
			console.log('Service Worker registered with scope: ', registration.scope);
		})
		.catch(function (error) {
			console.log('Service Worker registration failed:', error);
		});
}*/




class RecipeLink {
	static addLink(parent, url) {
		const button = document.createElement('button');
		button.classList = 'recipe-button';
		parent.appendChild(button);
		button.type = 'button';
		button.innerText = '🍳';
		button.onclick = () => { window.open(url, '_blank') }
		return button;
	}
}

class NotesPopup {
	static addLink(parent, notes) {
		const button = document.createElement('button');
		button.classList = 'recipe-button';
		parent.appendChild(button);
		button.type = 'button';
		button.innerText = '📝';
		const createNotes = () => {
			const noteWrapper = document.createElement('div');
			const noteElement = document.createElement('span')
			noteElement.innerText = notes
			noteWrapper.className = 'note-wrapper'
			noteWrapper.appendChild(noteElement)
			return noteWrapper
		}

		button.onclick = () => Popup.putOnOverLay(createNotes(), true, () => { Popup.closePopup(); })
		return button;
	}
}

var holders = document.querySelectorAll('.recipelinkplaceholder')
for (let i = 0; i < holders.length; i++) {
	const url = holders[i].getAttribute('linkurl');
	RecipeLink.addLink(holders[i], url);
}

var notesHolders = document.querySelectorAll('.notesplaceholder')
for (let i = 0; i < notesHolders.length; i++) {
	const notes = notesHolders[i].getAttribute('notes');
	NotesPopup.addLink(notesHolders[i], notes);
}

