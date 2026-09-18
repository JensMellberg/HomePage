class StarRanker {
	static create(callback, maxRanking, hideNote, currentNote, currentRanking) {
		const outer = HtmlUtils.createElement('div', 'star-wrapper-outer');
		const wrapper = HtmlUtils.addElement('div', 'star-wrapper', outer);
		maxRanking = maxRanking || 10
		let ranking = currentRanking
		for (let i = 1; i <= maxRanking; i++) {
			const star = HtmlUtils.addElement('span', 'star', wrapper)
			star.onclick = () => {
				ranking = i
				$('.star').removeClass('selected')
				star.classList.add('selected')
			}

			star.innerText = i.toString();
			if (i === currentRanking) {
				star.classList.add('selected')
			}
		}

		const textField = HtmlUtils.addElement('textarea', '', outer)
		if (hideNote) {
			textField.style.display = 'none'
		}

		textField.value = currentNote || ''

		Alert.openDialogWithElement(outer, [
			{ text: 'Klar', action: () => { callback(ranking, textField.value); Popup.closePopup(); } },
			{ text: 'Avbryt', action: () => { Popup.closePopup(); } }
		])

	}
}
