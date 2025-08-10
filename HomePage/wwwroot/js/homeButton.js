
useHomeButton();
function useHomeButton() {
	const button = document.createElement('button');
	button.className = 'home-button'
	button.textContent = 'Hem'
	button.onclick = () => window.location = '/index';
	const page = document.querySelectorAll('.page-wrapper, .calendar-wrapper')[0];
	page.insertBefore(button, page.firstElementChild);
}
